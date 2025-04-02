using System;
using System.Text;
using UnityEngine;
public class HostUpdater : IUpdater {
    StringBuilder sb;
    public void Initialize(GameInfo gameInfo) {
        gameInfo.Time = 60;
        gameInfo.Positions = new Vector2[4];
    }
    public void Update(GameInfo gameInfo) {
        gameInfo.Time -= Time.deltaTime;
    }
    public Message Package(GameInfo gameInfo, int id = 0) {
        sb ??= new();
        sb.Clear();
        sb.Append(gameInfo.Time);
        sb.Append('/');
        for (var i = 0; i < 4; i++) {
            sb.Append(gameInfo.Positions[i].x);
            sb.Append(',');
            sb.Append(gameInfo.Positions[i].y);
            sb.Append('/');
        }
        return new(MessageType.GameUpdate, id, sb.ToString());
    }
}

public class ClientUpdater : IUpdater {
    StringBuilder sb;
    public void Initialize(GameInfo gameInfo) {
        gameInfo.Positions = new Vector2[4];
    }
    public void Update(GameInfo gameInfo) {
        gameInfo.Time -= Time.deltaTime;
    }
    public Message Package(GameInfo gameInfo, int id = 0) {
        sb ??= new();
        sb.Clear();
        sb.Append(gameInfo.Positions[id].x);
        sb.Append(',');
        sb.Append(gameInfo.Positions[id].y);
        return new(MessageType.ClientUpdate, id, sb.ToString());
    }
}

public interface IUpdater {
    void Initialize(GameInfo gameInfo);
    void Update(GameInfo gameInfo);
    Message Package(GameInfo gameInfo, int id = 0);
}
