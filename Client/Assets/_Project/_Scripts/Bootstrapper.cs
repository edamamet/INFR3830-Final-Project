using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace _Project._Scripts {
    public class Bootstrapper : MonoBehaviour {
        public static async void LoadAdditive(int index) {
            var scene = SceneManager.GetSceneByBuildIndex(index);
            if (!scene.isLoaded) await SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        }
        public static async void ReplaceScene(int current, int next) {
            await SceneManager.UnloadSceneAsync(current);
            await SceneManager.LoadSceneAsync(next, LoadSceneMode.Additive);
        }
        void Awake() {
            LoadAdditive(1);
        }
    }
}