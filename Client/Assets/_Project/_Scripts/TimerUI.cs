using System.Globalization;
using TMPro;
using UnityEngine;
public class TimerUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text;
    void Update() {
        text.text = GameManager.GameInfo.Time.ToString("F2", CultureInfo.InvariantCulture);
    }
}