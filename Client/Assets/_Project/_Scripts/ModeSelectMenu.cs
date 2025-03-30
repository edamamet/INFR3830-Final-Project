using System.Threading.Tasks;
using UnityEngine;
namespace _Project._Scripts {
    public class ModeSelectMenu : MonoBehaviour, IMenu {
        [SerializeField] CanvasGroup canvas;
        [SerializeField] MainMenu mainMenu;
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

        public void ConfigureNetworkAsHost() {
            _ = ConfigureNetworkAsHostAsync();
        }

        async Task ConfigureNetworkAsHostAsync() {
            await mainMenu.StartNetworkClientAsync();
            NetworkConfiguration.Instance.SetMode(NetworkMode.Host);
        }
        
        public void ConfigureNetworkAsClient() {
            _ = ConfigureNetworkAsClientAsync();
        }
        
        async Task ConfigureNetworkAsClientAsync() {
            await mainMenu.StartNetworkClientAsync();
            NetworkConfiguration.Instance.SetMode(NetworkMode.Client);
        }
    }
}
