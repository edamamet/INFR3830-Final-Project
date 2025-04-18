using System;
using System.Collections.Generic;
using System.Net;
using _Project._Scripts;
using UnityEngine;

public class NetworkConfiguration : MonoBehaviour {
    public NetworkMode Mode { get; private set; }
    public INetworker Networker { get; private set; }
    public NetworkEntity[] Entities;
    public string Name;
    public int ID;

    public event Action<int, string> OnPlayerJoined = delegate { };
    public event Action<float, NetworkEntity[]> OnGameUpdate = delegate { };
    public event Action<int, Vector2> OnClientUpdate = delegate { };
    public event Action<Guid, Vector2> OnChestSpawned = delegate { };
    public event Action<int, Guid> OnChestOpened = delegate { };
    public event Action<int[]> OnGameOver = delegate { };

    Queue<Message> messageQueue;

    static NetworkConfiguration instance;
    public static NetworkConfiguration Instance {
        get {
            if (instance == null) {
                instance = FindAnyObjectByType<NetworkConfiguration>();
                if (instance == null) return null;
            }
            return instance;
        }
    }

    public void SetMode(NetworkMode mode) {
        Mode = mode;
    }

    public void Initialize(IPAddress address, string name) {
        Name = name;
        messageQueue = new();
        Entities = new NetworkEntity[4];
        Networker = Mode == NetworkMode.Host
            ? new HostNetworker(RequestPlayerNames)
            : new ClientNetworker();
        Networker.OnMessagePublished += OnMessagePublished;
        Networker.Initialize(address, name);
    }

    void OnDestroy() {
        if (Networker != null) {
            Networker.OnMessagePublished -= OnMessagePublished;
        }
    }

    void Update() {
        if (messageQueue == null) return;
        while(messageQueue.Count > 0) {
            Message message = messageQueue.Dequeue();
            Debug.Log($"Processing message: {message.Type}, {message.ID}, {message.Content}");
            switch(message.Type) {
                case MessageType.UserJoined: {
                    Debug.Log($"{Mode}, {ID}, {message.ID}");
                    if (Mode == NetworkMode.Client && ID == 0) {
                        ID = message.ID;
                    }
                    Entities[message.ID] = new(message.ID, Vector2.zero, message.Content);
                    OnPlayerJoined(message.ID, message.Content);
                }
                    break;
                case MessageType.RequestJoin:
                case MessageType.RequestPlayers: {
                    var entityNames = new string[4];
                    string[] names;
                    try {
                        names = message.Content.Split('/');
                    } catch {
                        continue;
                    }
                    for (var i = 0; i < 4; i++) {
                        entityNames[i] = i < names.Length ? names[i] : string.Empty;
                        if (Entities[i] == null && !string.IsNullOrEmpty(entityNames[i])) {
                            Entities[i] = new(i, Vector2.zero, entityNames[i]);
                            OnPlayerJoined(i, entityNames[i]);
                        }
                    }
                    if (Mode == NetworkMode.Client) {
                        for (int i = 0; i < 4; i++) {
                            if (Entities[i] == null) continue;
                            ID = i;
                        }
                    }
                }
                    break;
                case MessageType.UserLeft:
                case MessageType.StartGame:
                    Bootstrapper.ReplaceScene(1, 3);
                    break;
                case MessageType.GameUpdate: {
                    string[] parts;
                    try {
                        parts = message.Content.Split('/');
                    } catch {
                        continue;
                    }
                    var time = float.Parse(parts[0]);
                    for (var i = 0; i < 4; i++) {
                        if (Entities[i] == null) continue;
                        string[] pos;
                        try {
                            pos = parts[i + 1].Split(',');
                        } catch {
                            continue;
                        }
                        float x, y;
                        try {
                            x = float.Parse(pos[0]);
                            y = float.Parse(pos[1]);
                        } catch {
                            continue;
                        }
                        Entities[i].Position = new(x, y);
                    }
                    OnGameUpdate(time, Entities);
                }
                    break;
                case MessageType.ClientUpdate: {
                    string[] values;
                    try {
                        values = message.Content.Split(',');
                    } catch {
                        continue;
                    }
                    var x = float.Parse(values[0]);
                    var y = float.Parse(values[1]);
                    OnClientUpdate(message.ID, new(x, y));
                }
                    break;
                case MessageType.SpawnChest: {
                    string[] values;
                    try {
                        values = message.Content.Split('/');
                    } catch {
                        continue;
                    }
                    var id = Guid.Parse(values[0]);
                    var x = int.Parse(values[1]);
                    var y = int.Parse(values[2]);
                    OnChestSpawned(id, new(x, y));
                }
                    break;
                case MessageType.CollectChest: {
                    var id = Guid.Parse(message.Content);
                    OnChestOpened(message.ID, id);
                }
                    break;
                case MessageType.EndGame: {
                    string[] values;
                    values = message.Content.Split('/');
                    var scores = new int[4];
                    for (var i = 0; i < 4; i++) {
                        if (Entities[i] == null) continue;
                        try {
                            scores[i] = int.Parse(values[i]);
                        } catch {
                            continue;
                        }
                    }
                    OnGameOver(scores);
                }
                    break;
                default:
                    break;
            }
        }
    }

    void OnMessagePublished(Message message) {
        // we can't actually do anything here since we're in async land.
        // queue up the message for processing in the main thread.
        messageQueue.Enqueue(message);
    }

    public void RequestPlayerNames(ref string[] playerNames) {
        for (var i = 0; i < 4; i++) {
            playerNames[i] = Entities[i]?.Name ?? string.Empty;
        }
    }

    public void UpdateGame(IUpdater updater, GameInfo gameInfo) {
        var message = updater.Package(gameInfo, ID);
        Networker.MakeRequest(message);
    }
}
