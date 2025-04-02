using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
public class ChestManager : MonoBehaviour {
    [SerializeField] float spawnInterval;
    [SerializeField] Chest chestPrefab;
    [SerializeField] Tilemap floorTilemap;
    Dictionary<Guid, Chest> chests = new();
    List<Vector3Int> spawnPoints = new();
    float currentTime;
    public bool IsActive = false;
    void Awake() {
        chests ??= new();
        chests.Clear();
        
        foreach (Vector3Int pos in floorTilemap.cellBounds.allPositionsWithin) {
            if (floorTilemap.HasTile(pos)) {
                spawnPoints.Add(pos);
            }
        }
    }
    public void RegisterChest(Chest chest) => chests.TryAdd(chest.ID, chest);
    public void RemoveChest(Chest chest) => chests.Remove(chest.ID);

    void Update() {
        if (!IsActive) return;
        currentTime += Time.deltaTime;
        if (currentTime < spawnInterval) return;
        currentTime = 0;
        Vector3Int spawnPos = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Chest chest = Instantiate(chestPrefab, spawnPos, Quaternion.identity);
        chest.Initialize(this);
    }
}