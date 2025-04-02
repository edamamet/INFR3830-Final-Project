using System;
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

    void Awake() {
        GameInfo = new();
    }

    void OnEnable() {
        if (!Network) return;
        if (Network.Mode == NetworkMode.Host) {
            Network.OnClientUpdate += OnClientUpdate;
        } else {
            Network.OnGameUpdate += OnGameUpdate;
        }
    }
    void OnDisable() {
        if (!Network) return;
        if (Network.Mode == NetworkMode.Host) {
            Network.OnClientUpdate -= OnClientUpdate;
        } else {
            Network.OnGameUpdate -= OnGameUpdate;
        }
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

    void OnClientUpdate(int id, Vector2 position) {
        Debug.Log($"Setting position for {id} to {position}");
        GameInfo.Positions[id] = position;
        GameInfoChanged(GameInfo);
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
