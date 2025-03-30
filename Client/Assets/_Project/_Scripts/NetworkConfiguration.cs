using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class NetworkConfiguration : MonoBehaviour {
    public NetworkMode Mode { get; private set; }
    public INetworker Networker { get; private set; }
    public NetworkEntity[] Entities;
    public string Name;

    public event Action<int, string> OnPlayerJoined = delegate { };
    public event Action<string[]> OnRequestPlayers = delegate { };

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
            ? new HostNetworker()
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
                    }
                    OnRequestPlayers(entityNames);
                    break;
                case MessageType.UserLeft:
                case MessageType.StartGame:
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
}
