using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Relay : MonoBehaviour {
    bool active = false;
    float time = 0;
    [SerializeField] float interval = 1;
    [SerializeField] Slider intervalSettingSlider;
    [SerializeField] TextMeshProUGUI intervalSettingText;
    
    IUpdater Updater => GameManager.Updater;
    GameInfo GameInfo => GameManager.GameInfo;
    void Start() {
        if (NetworkConfiguration.Instance) active = true;
        SetInterval(interval);
    }

    void Update() {
        if (!active) return;
        ProcessGame();
    }

    void ProcessGame() {
        time += Time.deltaTime;
        if (time < interval) return;
        time = 0;
        NetworkConfiguration.Instance.UpdateGame(Updater, GameInfo);
    }
    
    public void SetInterval(float value) {
        interval = value;
        intervalSettingText.text = value.ToString("F2", CultureInfo.InvariantCulture);
    }
}