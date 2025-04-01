using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
    Queue<Message> sendQueue = null, receiveQueue = null;
    public event Action<Message> OnMessagePublished = delegate { };
    public void Initialize(IPAddress address, string name) {
        Debug.Log("Initializing Host");

        buffer = new byte[1024];
        tcpClients = new Socket[4];
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
        Message.Decode(buffer, receiveQueue, bytes);
        foreach (Message message in receiveQueue) {
            Debug.Log($"Received {bytes} bytes: {message.Type} from {client.RemoteEndPoint}: {message.Content}");
            switch(message.Type) {
                case MessageType.RequestJoin:
                    HandleRequestJoin(client, message.Content);
                    break;
                default:
                    Debug.Log($"Unhandled message of type: {message.Type}");
                    break;
            }
        }
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, client);
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
                        Debug.Log($"Sent {bytes} bytes to {message.Type}: {message.Content}");
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
    }
    void OnConnect(IAsyncResult result) {
        client.EndConnect(result);
        Debug.Log($"Connected to {client.RemoteEndPoint}");

        QueueMessage(new(MessageType.RequestJoin, 0, name));
        QueueMessage(new(MessageType.RequestPlayers, 0, string.Empty));
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
        _ = SendLoop();
    }
    void OnSend(Message message) {
        Debug.Log("Sent");
    }
    void OnReceive(IAsyncResult result) {
        int received = client.EndReceive(result);
        Message.Decode(buffer, receiveQueue, received);
        foreach (Message message in receiveQueue) {
            OnMessagePublished(message);
            Debug.Log($"Received {received} bytes: {message.Type} {message.Content}");
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
        }
    }

    void QueueMessage(Message message) {
        sendQueue.Enqueue(message);
    }

    async Task SendLoop() {
        for (;;) {
            while(sendQueue is { Count: > 0 }) {
                Message message = sendQueue.Dequeue();
                int bytes = message.Encode(buffer);
                ArraySegment<byte> segment = new(buffer, 0, bytes);
                Debug.Log($"Sending {bytes} bytes: {message.Type} {message.Content}");
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
        QueueMessage(message);
    }
}
