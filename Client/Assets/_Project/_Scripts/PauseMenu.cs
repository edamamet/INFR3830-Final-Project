using System.Collections.Generic;
using _Project._Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
public class PauseMenu : MonoBehaviour, IMenu {
    [SerializeField] CanvasGroup canvas;
    [SerializeField] SettingsMenu settingsMenu;
    public static bool IsPaused = false;

    Stack<IMenu> menus = new();

    void Awake() {
        Hide();
        settingsMenu.Hide();
        IsPaused = false;
    }

    void Update() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            if (menus.Count == 0) {
                IsPaused = true;
                PushMenu(this);
            } else {
                PopMenu();
                if (menus.Count == 0) IsPaused = false;
            }
        }
    }
    public void PushMenu(IMenu menu) {
        if (menus.Count > 0) menus.Peek().Hide();
        menus.Push(menu);
        menu.Show();
    }

    public void PopMenu() {
        if (menus.Count == 0) return;
        menus.Pop().Hide();
        if (menus.Count > 0) menus.Peek().Show();
    }

    public void PushSettingsMenu() => PushMenu(settingsMenu);

    public void Quit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void Show() {
        canvas.Show();
    }
    public void Hide() {
        canvas.Hide();
    }
    public void ForceShow() {
        canvas.Show();
    }
    public void ForceHide() {
        canvas.Hide();
    }
}
