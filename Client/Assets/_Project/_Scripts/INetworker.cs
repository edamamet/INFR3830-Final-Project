using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
    public event Action<Message> OnMessagePublished = delegate { };
    public void Initialize(IPAddress address, string name) {
        Debug.Log("Initializing Host");

        buffer = new byte[1024];
        tcpClients = new Socket[4];

        server = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(address, 8888));
        server.Listen(100);

        HandleRequestJoin(server, name);

        Debug.Log($"Listening for connections on {address}:8888");
        server.BeginAccept(OnAccept, null);
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
        var message = Message.Decode(buffer, bytes);
        Debug.Log($"Received {bytes} bytes: {message.Type} from {client.RemoteEndPoint}: {message.Content}");
        switch(message.Type) {
            case MessageType.RequestJoin:
                HandleRequestJoin(client, message.Content);
                break;
            default: break;
        }
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, client);
    }

    void HandleRequestJoin(Socket client, string name) {
        var index = 0;
        while(tcpClients[index] != null) index++;
        tcpClients[index] = client;
        Message response = new(MessageType.UserJoined, index, name);
        int responseBytes = response.Encode(buffer);
        for (var i = 1; i < 4; i++) {
            Socket tcpClient = tcpClients[i];
            tcpClient?.BeginSend(buffer, index, responseBytes, SocketFlags.None, null, null);
        }
        OnMessagePublished(response);
    }

    public void Dispose() {
        Debug.Log("Disposing host");
        OnMessagePublished = null;
        server.Close();
    }
    public void MakeRequest(Message message) {
        int bytes = message.Encode(buffer);
        for (var i = 1; i < 4; i++) {
            Socket tcpClient = tcpClients[i];
            tcpClient?.BeginSend(buffer, 0, bytes, SocketFlags.None, null, null);
        }
    }
}

public class ClientNetworker : INetworker {
    Socket client;
    string name;
    byte[] buffer;

    public event Action<Message> OnMessagePublished = delegate { };
    public void Initialize(IPAddress address, string name) {
        Debug.Log("Initializing Client");
        buffer = new byte[1024];
        this.name = name;
        client = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        client.BeginConnect(new IPEndPoint(address, 8888), OnConnect, null);
    }
    void OnConnect(IAsyncResult result) {
        client.EndConnect(result);
        Debug.Log($"Connected to {client.RemoteEndPoint}");
        Message message = new(MessageType.RequestJoin, 0, name);
        int bytes = message.Encode(buffer);
        Debug.Log($"Sending {bytes} bytes: {message.Type} {message.Content}");
        client.BeginSend(buffer, 0, bytes, SocketFlags.None, OnSend, client);
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
    }
    void OnSend(IAsyncResult result) {
        Debug.Log("Sent");
        client.EndSend(result);
    }
    void OnReceive(IAsyncResult result) {
        int received = client.EndReceive(result);
        var message = Message.Decode(buffer, received);
        Debug.Log($"Received {received} bytes: {message.Type} {message.Content}");
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
    }
    public void Dispose() {
        Debug.Log("Disposing client");
        OnMessagePublished = null;
        client.Close();
    }
    public void MakeRequest(Message message) {
        int bytes = message.Encode(buffer);
        client.BeginSend(buffer, 0, bytes, SocketFlags.None, OnSend, client);
    }
}