using System.Text;
public struct Message {
    public MessageType Type;
    public int ID;
    public string Content;
    public Message(MessageType type, int id, string content) {
        Type = type;
        ID = id;
        Content = content;
    }

    public int Encode(byte[] buffer) {
        buffer[0] = (byte)Type;
        buffer[1] = (byte)ID;
        int bytes = Encoding.UTF8.GetBytes(Content, 0, Content.Length, buffer, 2);
        return bytes + 2;
    }

    public static Message Decode(byte[] buffer, int size) {
        var type = (MessageType)buffer[0];
        int id = buffer[1];
        string content = Encoding.UTF8.GetString(buffer, 2, size - 2);
        return new(type, id, content);
    }
}

public enum MessageType : byte {
    RequestJoin,
    RequestPlayers,
    UserJoined,
    UserLeft,
    StartGame,
}
