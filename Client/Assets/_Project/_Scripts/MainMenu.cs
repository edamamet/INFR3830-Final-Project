using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
namespace _Project._Scripts {
    public class MainMenu : MonoBehaviour {
        [SerializeField] HomeMenu homeMenu;
        [SerializeField] PregameMenuHost pregameMenuHost;
        [SerializeField] ModeSelectMenu modeSelectMenu;
        [SerializeField] LobbyMenu lobbyMenu;
        [SerializeField] SettingsMenu settingsMenu;

        [SerializeField] EventReference clickSound;
        readonly Stack<IMenu> menuStack = new();

        void Start() {
            homeMenu.gameObject.SetActive(true);
            pregameMenuHost.gameObject.SetActive(true);
            modeSelectMenu.gameObject.SetActive(true);
            lobbyMenu.gameObject.SetActive(true);
            settingsMenu.gameObject.SetActive(true);
            
            // TODO: remove
            settingsMenu.SetMusicVolume(0);

            homeMenu.ForceHide();
            pregameMenuHost.ForceHide();
            modeSelectMenu.ForceHide();
            lobbyMenu.ForceHide();
            settingsMenu.ForceHide();

            PushMenu(homeMenu);
        }

        void Update() {
            if (Keyboard.current.escapeKey.wasPressedThisFrame && menuStack.Count > 1) {
                PopMenu();
                RuntimeManager.PlayOneShot(clickSound);
            }
        }

        public void PushHomeMenu() {
            PushMenu(homeMenu);
        }

        public void PushPregameMenu() {
            PushMenu(pregameMenuHost);
        }

        public void PushModeSelectMenu() {
            PushMenu(modeSelectMenu);
        }

        public void PushLobbyMenu() {
            PushMenu(lobbyMenu);
        }
        public void PushSettingsMenu() {
            PushMenu(settingsMenu);
        }

        public void PushMenu(IMenu menu) {
            if (menuStack.Count > 0) menuStack.Peek().Hide();
            menuStack.Push(menu);
            menu.Show();
        }

        public void PopMenu() {
            if (menuStack.Count == 0) return;
            menuStack.Pop().Hide();
            if (menuStack.Count > 0) menuStack.Peek().Show();
        }

        public void StartNetworkClient() {
            _ = StartNetworkClientAsync();
        }
        public async Task StartNetworkClientAsync() {
            await SceneManager.LoadSceneAsync(sceneBuildIndex: 1, LoadSceneMode.Additive);
        }

        public void KillNetworkClient() {
            _ = KillNetworkClientAsync();
        }
        public async Task KillNetworkClientAsync() {
            await SceneManager.UnloadSceneAsync(sceneBuildIndex: 1, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        }

        public void Quit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
