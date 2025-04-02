using System;
using UnityEngine;
class NetworkPlayer : MonoBehaviour {
    public Vector2 Position => transform.position;
    public int ID;

    Vector2 lastPosition;
    
    void Update() {
        if (!NetworkConfiguration.Instance) return;
        Vector2 position = GameManager.GameInfo.Positions[ID];
        transform.position = position;
        lastPosition = position;
    }
}