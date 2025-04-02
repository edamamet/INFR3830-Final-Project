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
                case MessageType.UserJoined:
                    Entities[message.ID] = new(message.ID, Vector2.zero, message.Content);
                    OnPlayerJoined(message.ID, message.Content);
                    break;
                case MessageType.RequestJoin:
                case MessageType.RequestPlayers:
                    var entityNames = new string[4];
                    string[] names = message.Content.Split('/');
                    for (var i = 0; i < 4; i++) {
                        entityNames[i] = i < names.Length ? names[i] : string.Empty;
                        if (Entities[i] == null && !string.IsNullOrEmpty(entityNames[i])) {
                            Entities[i] = new(i, Vector2.zero, entityNames[i]);
                            OnPlayerJoined(i, entityNames[i]);
                        }
                    }
                    break;
                case MessageType.UserLeft:
                case MessageType.StartGame:
                    Bootstrapper.ReplaceScene(1,3);
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
        for (int i = 0; i < 4; i++) {
            playerNames[i] = Entities[i]?.Name ?? string.Empty;
        }
    }
}
