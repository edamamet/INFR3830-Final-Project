using System.Collections.Generic;
using System.Text;
using UnityEngine;
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
        int bytes = Encoding.ASCII.GetBytes(Content + '\\', 0, Content.Length + 1, buffer, 2);
        return bytes + 2;
    }

    public static void Decode(byte[] buffer, Queue<Message> messageQueue, int size) {
        Debug.Log($"decoding..");
        int index = 0;
        while(index > -1 && index < size) {
            var type = (MessageType)buffer[index];
            int id = buffer[index + 1];
            string content = Encoding.ASCII.GetString(buffer, index + 2, size - index - 2);
            index = content.IndexOf('\\');
            if (index >= 0) {
                if (content.Length > 1) {
                    content = content.Substring(0, index);
                    index += 3;
                } else {
                    Debug.Log("Content length is 1");
                    index = int.MaxValue;
                }
            }
            messageQueue.Enqueue(new(type, id, content));
        }
    }
}

public enum MessageType : byte {
    RequestJoin,
    RequestPlayers,
    UserJoined,
    UserLeft,
    StartGame,
    EndGame,
    GameUpdate,
    ClientUpdate,   
}