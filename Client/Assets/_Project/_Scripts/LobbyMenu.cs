using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace _Project._Scripts {
    public class LobbyMenu : MonoBehaviour, IMenu {
        [SerializeField] CanvasGroup canvas;
        [SerializeField] LobbyPlayerUI[] playerUis;
        [SerializeField] Button startButton;
        public void Show() {
            if (NetworkConfiguration.Instance) {
                NetworkConfiguration.Instance.OnPlayerJoined += OnPlayerJoined;
                foreach (var playerUi in playerUis) playerUi.Unset();
            }
            bool isStartButtonActive = NetworkConfiguration.Instance.Mode == NetworkMode.Host;
            startButton.interactable = isStartButtonActive;
            startButton.gameObject.SetActive(isStartButtonActive);
            canvas.Show();
        }
        public void Hide() {
            if (NetworkConfiguration.Instance) {
                NetworkConfiguration.Instance.OnPlayerJoined -= OnPlayerJoined;
                NetworkConfiguration.Instance.Networker?.Dispose();
            }
            canvas.Hide();
        }
        public void ForceShow() {
            canvas.Show();
        }
        public void ForceHide() {
            if (NetworkConfiguration.Instance)
                NetworkConfiguration.Instance.Networker?.Dispose();
            canvas.Hide();
        }
        void OnDestroy() {
            Hide();
        }

        void OnPlayerJoined(int id, string name) {
            Debug.Log($"Lobby: player joined {id}, {name}");
            playerUis[id].Set(name);
        }
    }
}
