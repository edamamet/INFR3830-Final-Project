using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
public class ChestManager : MonoBehaviour {
    [SerializeField] float spawnInterval;
    [SerializeField] Chest chestPrefab;
    [SerializeField] Tilemap floorTilemap;
    Dictionary<Guid, Chest> chests = new();
    List<Vector3Int> spawnPoints = new();
    NetworkConfiguration Network => NetworkConfiguration.Instance;
    float currentTime;
    bool IsHost => Network && Network.Mode == NetworkMode.Host;
    StringBuilder sb;
    void Awake() {
        chests ??= new();
        chests.Clear();
        
        foreach (Vector3Int pos in floorTilemap.cellBounds.allPositionsWithin) {
            if (floorTilemap.HasTile(pos)) {
                spawnPoints.Add(pos);
            }
        }
        sb ??= new();
    }
    public void RegisterChest(Chest chest) => chests.TryAdd(chest.ID, chest);
    public void RemoveChest(Chest chest) => chests.Remove(chest.ID);

    void Update() {
        if (!IsHost) return;
        currentTime += Time.deltaTime;
        if (currentTime < spawnInterval) return;
        currentTime = 0;
        Vector3Int spawnPos = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Chest chest = Instantiate(chestPrefab, spawnPos, Quaternion.identity, transform);
        chest.Initialize(this);

        sb.Clear();
        sb.Append(chest.ID.ToString());
        sb.Append('/');
        sb.Append(spawnPos.x);
        sb.Append('/');
        sb.Append(spawnPos.y);
        Network.Networker.MakeRequest(new(MessageType.SpawnChest, 0, sb.ToString()));
    }

    public void SpawnChest(Guid id, Vector2 position) {
        Chest chest = Instantiate(chestPrefab, position, Quaternion.identity, transform);
        chest.Initialize(this, id);
    }
}