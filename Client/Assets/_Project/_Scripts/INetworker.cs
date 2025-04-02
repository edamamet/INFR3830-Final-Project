using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
public interface INetworker {
    event Action<Message> OnMessagePublished;
    void Initialize(IPAddress address, string name);
    void Dispose();
    void MakeRequest(Message message);
}

public class HostNetworker : INetworker {
    byte[] buffer = new byte[1024];
    Socket server;
    Socket[] tcpClients;
    string[] playerNames;
    Queue<Message> sendQueue = null, receiveQueue = null;
    public event Action<Message> OnMessagePublished = delegate { };
    public delegate void RequestPlayerNamesDelegate(ref string[] playerNameBuffer);
    public readonly RequestPlayerNamesDelegate RequestPlayerNames;

    public HostNetworker(RequestPlayerNamesDelegate requestPlayerNames) {
        RequestPlayerNames = requestPlayerNames;
    }
    public void Initialize(IPAddress address, string name) {
        Debug.Log("Initializing Host");

        buffer = new byte[1024];
        tcpClients = new Socket[4];
        playerNames = new string[4];
        sendQueue = new();
        receiveQueue = new();

        server = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(address, 8888));
        server.Listen(100);

        HandleRequestJoin(server, name);

        Debug.Log($"Listening for connections on {address}:8888");
        server.BeginAccept(OnAccept, null);

        _ = SendLoop();
    }
    void OnAccept(IAsyncResult result) {
        Socket client = server.EndAccept(result);
        Debug.Log($"Connected with {client.RemoteEndPoint}");
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, client);
        server.BeginAccept(OnAccept, null);
    }

    void OnReceive(IAsyncResult result) {
        if (result.AsyncState is not Socket client) return;
        int bytes = client.EndReceive(result);
        if (bytes == 0) {
            Debug.Log($"Received a blank message. Disconnecting from {client.RemoteEndPoint}");
            client.Close();
            return;
        }
        Message.Decode(buffer, receiveQueue, bytes);
        Debug.Log($"Received {bytes} bytes as {receiveQueue.Count} messages from {client.RemoteEndPoint}");
        while(receiveQueue.Count > 0) {
            Message message = receiveQueue.Dequeue();
            Debug.Log($"  >  {message.Type} from {client.RemoteEndPoint}: {message.Content}");
            switch(message.Type) {
                case MessageType.RequestJoin:
                    HandleRequestJoin(client, message.Content);
                    break;
                case MessageType.RequestPlayers:
                    HandleRequestPlayers();
                    break;
                default:
                    Debug.Log($"Performing default action for {message.Type}");
                    message.ID = Array.IndexOf(tcpClients, client);
                    OnMessagePublished(message);
                    break;
            }
        }
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, client);
    }

    void HandleRequestPlayers() {
        RequestPlayerNames(ref playerNames);
        string content = string.Join("/", playerNames);
        Message response = new(MessageType.RequestPlayers, 0, content);
        QueueMessage(response);
        OnMessagePublished(response);
    }

    void HandleRequestJoin(Socket client, string name) {
        var index = 0;
        while(tcpClients[index] != null) index++;
        tcpClients[index] = client;
        Message response = new(MessageType.UserJoined, index, name);
        QueueMessage(response);
        OnMessagePublished(response);
    }

    void QueueMessage(Message message) {
        Debug.Log($"Message queued: [{message.ID}] {message.Type}: {message.Content}");
        sendQueue.Enqueue(message);
    }

    async Task SendLoop() {
        for (;;) {
            while(sendQueue is { Count: > 0, }) {
                Message message = sendQueue.Dequeue();
                int bytes = message.Encode(buffer);
                ArraySegment<byte> segment = new(buffer, 0, bytes);
                for (var i = 1; i < 4; i++) {
                    Socket tcpClient = tcpClients[i];
                    if (tcpClient != null) {
                        await tcpClient.SendAsync(segment, SocketFlags.None);
                        Debug.Log($"Sent {bytes} bytes to {tcpClient}: {message.ID}{message.Type}: {message.Content}");
                    }
                }
            }
            await Awaitable.NextFrameAsync();
        }
    }

    public void Dispose() {
        Debug.Log("Disposing host");
        OnMessagePublished = null;
        server.Close();
    }
    public void MakeRequest(Message message) {
        QueueMessage(message);
    }
}

public class ClientNetworker : INetworker {
    Socket client;
    string name;
    byte[] buffer;
    Queue<Message> sendQueue = null, receiveQueue = null;

    public event Action<Message> OnMessagePublished = delegate { };
    public void Initialize(IPAddress address, string name) {
        Debug.Log("Initializing Client");
        buffer = new byte[1024];
        sendQueue = new();
        receiveQueue = new();
        this.name = name;
        client = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        client.BeginConnect(new IPEndPoint(address, 8888), OnConnect, null);
        _ = SendLoop();
    }
    void OnConnect(IAsyncResult result) {
        client.EndConnect(result);
        Debug.Log($"Connected to {client.RemoteEndPoint}");

        QueueMessage(new(MessageType.RequestJoin, 0, name));
        QueueMessage(new(MessageType.RequestPlayers, 0, string.Empty));
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
    }
    void OnSend(Message message) {
        Debug.Log("Sent");
    }
    void OnReceive(IAsyncResult result) {
        try {
            int received = client.EndReceive(result);
            if (received == 0) {
                Debug.Log("Received 0 bytes. The server has disconnected.");
                try {
                    client.Close();
                } catch {
                    // ignored (the server may have already tried closing this client)
                }
                return;
            }
            Debug.Log($"Received {received} bytes");
            Message.Decode(buffer, receiveQueue, received);
            while(receiveQueue.Count > 0) {
                Message message = receiveQueue.Dequeue();
                OnMessagePublished(message);
                Debug.Log($"Received {received} bytes: {message.Type} {message.Content}");
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
            }
        } catch (Exception e) {
            Debug.Log($"Error {e.Message}");
            client.Close();
        }
    }

    void QueueMessage(Message message) {
        sendQueue.Enqueue(message);
        Debug.Log($"Message queued: [{message.ID}] {message.Type}: {message.Content}\n  >  {sendQueue.Count} messages in queue");
    }

    async Task SendLoop() {
        for (;;) {
            while(sendQueue is { Count: > 0 }) {
                Message message = sendQueue.Dequeue();
                int bytes = message.Encode(buffer);
                ArraySegment<byte> segment = new(buffer, 0, bytes);
                Debug.Log($"Sending {bytes} bytes: [{message.ID}] {message.Type} {message.Content}");
                await client.SendAsync(segment, SocketFlags.None);
                OnSend(message);
            }
            await Awaitable.NextFrameAsync();
        }
    }

    public void Dispose() {
        Debug.Log("Disposing client");
        OnMessagePublished = null;
        client.Close();
    }

    public void MakeRequest(Message message) {
        Debug.Log("CLIENT REQUEST");
        QueueMessage(message);
    }
}
