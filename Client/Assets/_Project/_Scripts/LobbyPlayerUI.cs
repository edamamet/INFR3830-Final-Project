using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace _Project._Scripts {
    public class LobbyPlayerUI : MonoBehaviour {
        [SerializeField] int id;
        [SerializeField] TextMeshProUGUI playerId, playerName;
        [SerializeField] Image image;

        void OnValidate() {
            playerId.text = $"player {id}:";
        }

        public void Unset() {
            playerName.text = string.Empty;
            Color color = image.color;
            color.a = 0.5f;
            image.color = color;
        }

        public void Set(string name) {
            playerName.text = name;
            Color color = image.color;
            color.a = 1f;
            image.color = color;
        }
    }
}
