using System;
using System.Net;
using TMPro;
using UnityEngine;
namespace _Project._Scripts {
    public class PregameMenuHost : MonoBehaviour, IMenu {
        [SerializeField] CanvasGroup canvas;
        [SerializeField] MainMenu mainMenu;
        [SerializeField] TMP_InputField nameInputField,
                                        addressInputField;
        public void Show() {
            canvas.Show();
        }
        public void Hide() {
            canvas.Hide();
        }

        public void ForceShow() => canvas.Show();
        public void ForceHide() => canvas.Hide();

        public void ValidateInputs() {
            if (string.IsNullOrWhiteSpace(nameInputField.text)
                || nameInputField.text.Contains('/')
                || nameInputField.text == "Invalid Name") {
                nameInputField.text = "Invalid Name";
                return;
            }

            IPAddress ip;

            try {
                ip = IPAddress.Parse(addressInputField.text);
            } catch (Exception e) {
                addressInputField.text = "Invalid Address";
                Debug.LogError(e);
                return;
            }

            mainMenu.PushLobbyMenu();
            NetworkConfiguration.Instance.Initialize(ip, nameInputField.text);
        }

        public void SetLoopback() {
            addressInputField.text = IPAddress.Loopback.ToString();
        }
    }
}
