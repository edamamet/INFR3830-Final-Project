using Unity.Burst.Intrinsics;
using UnityEngine;
public class Interpolator : MonoBehaviour {
    float lastCachedTime, timeRate, lastReceivedTime;
    Vector2[] lastCachedPositions = new Vector2[4], velocities = new Vector2[4];

    NetworkConfiguration Network => NetworkConfiguration.Instance;
    [SerializeField] bool isActive = true;
    bool isActiveBacking;
    void OnEnable() {
        if (!Network) return;
        GameManager.GameInfoChanged += OnGameInfoChanged;
        lastCachedTime = 60;
        lastReceivedTime = Time.time;
    }

    void OnDisable() {
        if (!Network) return;
        GameManager.GameInfoChanged -= OnGameInfoChanged;
    }

    void OnGameInfoChanged(GameInfo gameInfo) {
        if (Network.Mode == NetworkMode.Client) {
            float delta = Time.time - lastReceivedTime;
            timeRate = gameInfo.Time - lastCachedTime / (delta == 0 ? 1 : delta);
            lastCachedTime = gameInfo.Time;
        }
        for (int i = 0; i < 4; i++) {
            if (i == Network.ID) continue;
            if (Network.Entities[i] == null) continue;
            Vector2 position = gameInfo.Positions[i];
            float delta = Time.time - lastReceivedTime;
            velocities[i] = (position - lastCachedPositions[i]) / (delta == 0 ? 1 : delta);
            lastCachedPositions[i] = position;
        }
        lastReceivedTime = Time.time;
    }

    void Update() {
        if (!isActive || !Network) return;
        GameManager.GameInfo.Time += timeRate * Time.deltaTime;
        for (int i = 0; i < 4; i++) {
            if (i == Network.ID) continue;
            if (Network.Entities[i] == null) continue;
            GameManager.GameInfo.Positions[i] += velocities[i] * Time.deltaTime;
        }
    }
}
