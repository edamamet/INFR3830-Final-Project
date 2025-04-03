using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
public class Leaderboard : MonoBehaviour {
    public IList<LeaderboardEntry> LoadLeaderboard() {
        string path = Path.Combine(Application.persistentDataPath, "leaderboard.json");
        if (!File.Exists(path)) {
            Debug.Log("Leaderboard file not found, creating a new one.");
            return new List<LeaderboardEntry>();
        }
        string json = File.ReadAllText(path);
        var entries = JsonUtility.FromJson<List<LeaderboardEntry>>(json);
        Debug.Log($"Loaded {entries.Count} leaderboard entries.");
        return entries;
    }

    public void SaveLeaderboard(IList<LeaderboardEntry> entries) {
        string path = Path.Combine(Application.persistentDataPath, "leaderboard.json");
        var topEntries = entries.OrderByDescending(entry => entry.Score).Take(10).ToArray();
        string json = JsonUtility.ToJson(topEntries);
        File.WriteAllText(path, json);
        Debug.Log($"Saved {topEntries.Length} leaderboard entries.");
    }

    public string Parse(LeaderboardEntry[] entries) {
        StringBuilder sb = new();
        for (var i = 0; i < entries.Length; i++) {
            sb.Append($"#{i + 1}: {entries[i].Name} - {entries[i].Score}");
            if (i < entries.Length - 1) sb.Append('\n');
        }
        return sb.ToString();
    }
}
