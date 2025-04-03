using System;
using System.Linq;
using System.Text;
using FMODUnity;
using TMPro;
using UnityEngine;
public class GameOverUI : MonoBehaviour {
    [SerializeField] EventReference gameOverSound, celebrationSoundEffect;
    [SerializeField] StudioEventEmitter musicSoundEmitter;
    [SerializeField] CanvasGroup gameOverCanvas;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Leaderboard leaderboard;
    public void Start() {
        gameOverCanvas.Hide();
    }
    public async void Show(int[] scoreArray) {
        gameOverCanvas.Show();
        musicSoundEmitter.Stop();
        RuntimeManager.PlayOneShot(gameOverSound);
        
        await Awaitable.WaitForSecondsAsync(3f);

        StringBuilder sb = new();
        var entries = leaderboard.LoadLeaderboard();
        scoreArray = scoreArray.OrderByDescending(x => x).ToArray();
        for (var i = 0; i < scoreArray.Length; i++) {
            if (NetworkConfiguration.Instance.Entities[i] == null) continue;
            var entityName = NetworkConfiguration.Instance.Entities[i].Name;
            var score = scoreArray[i];
            sb.Append($"#{i + 1}: {entityName} - {score}");
            entries.Add(new(entityName, score));
            if (i < scoreArray.Length - 1) sb.Append('\n');
        }
        RuntimeManager.PlayOneShot(celebrationSoundEffect);
        musicSoundEmitter.Play();
        text.text = sb.ToString();

        entries = entries.OrderByDescending(x => x.Score).ToList();
        leaderboard.SaveLeaderboard(entries);
        
        await Awaitable.WaitForSecondsAsync(5f);

        text.fontSize = 100;
        text.text = $"LEADERBOARD\n{leaderboard.Parse(entries.ToArray())}";
    }
}
