﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour {
    public static GameInfo GameInfo;
    public static IUpdater Updater;
    public static event Action<GameInfo> GameInfoChanged = delegate { };

    public bool isActive = false;

    [SerializeField] NetworkPlayer playerPrefab, self;
    [SerializeField] ChestManager chestManager;

    NetworkConfiguration Network => NetworkConfiguration.Instance;
    public static int Score = 0;
    int[] scores;

    void Awake() {
        GameInfo = new();
        scores = new int[4];
        for (var i = 0; i < 4; i++) { scores[i] = 0; }
    }

    void OnEnable() {
        if (!Network) return;
        if (Network.Mode == NetworkMode.Host) {
            Network.OnClientUpdate += OnClientUpdate;
        } else {
            Network.OnGameUpdate += OnGameUpdate;
            Network.OnChestSpawned += OnChestSpawned;
        }
        Network.OnChestOpened += OnChestOpened;
    }
    void OnDisable() {
        if (!Network) return;
        if (Network.Mode == NetworkMode.Host) {
            Network.OnClientUpdate -= OnClientUpdate;
        } else {
            Network.OnGameUpdate -= OnGameUpdate;
            Network.OnChestSpawned -= OnChestSpawned;
        }
        Network.OnChestOpened -= OnChestOpened;
    }

    void OnGameUpdate(float time, NetworkEntity[] entities) {
        Debug.Log("Updating game info");
        GameInfo.Time = time;
        for (int i = 0; i < entities.Length; i++) {
            if (entities[i] == null || i == Network.ID) continue;
            GameInfo.Positions[i] = entities[i].Position;
        }
        GameInfoChanged(GameInfo);
    }

    void OnChestSpawned(Guid id, Vector2 position) {
        chestManager.SpawnChest(id, position);
    }
    void OnClientUpdate(int id, Vector2 position) {
        Debug.Log($"Setting position for {id} to {position}");
        GameInfo.Positions[id] = position;
        GameInfoChanged(GameInfo);
    }
    void OnChestOpened(int id, Guid guid) {
        chestManager.OpenChest(guid);
        scores[id]++;
    }

    void Start() {
        if (!Network) return;
        isActive = true;
        Updater = Network.Mode switch {
            NetworkMode.Host => new HostUpdater(),
            NetworkMode.Client => new ClientUpdater(),
            _ => throw new NotImplementedException(),
        };
        Debug.Log($"GameManager: Initialized {Updater.GetType().Name}");

        self.ID = Network.ID;

        for (int i = 0; i < 4; i++) {
            if (i == Network.ID || Network.Entities[i] == null) continue;
            Debug.Log("Spawning player");
            NetworkPlayer player = Instantiate(playerPrefab, transform);
            player.ID = i;
        }

        Updater.Initialize(GameInfo);
    }

    void Update() {
        if (!isActive) return;
        GameInfo.Positions[self.ID] = self.Position;
        Updater.Update(GameInfo);
    }
}
