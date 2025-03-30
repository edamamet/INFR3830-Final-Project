using UnityEngine;
public class NetworkEntity {
    public int ID;
    public Vector2 Position;
    public string Name;
    
    public NetworkEntity(int id, Vector2 position, string name) {
        ID = id;
        Position = position;
        Name = name;
    }
}
