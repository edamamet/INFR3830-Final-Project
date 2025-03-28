using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class PauseMenu : MonoBehaviour {
    [SerializeField] CanvasGroup pauseMenu;
    bool isPaused = false;

    void Awake() {
        pauseMenu.Hide();
    }

    void Update() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            isPaused = !isPaused;
            if (isPaused) pauseMenu.Show();
            else pauseMenu.Hide();
        }
    }
}
