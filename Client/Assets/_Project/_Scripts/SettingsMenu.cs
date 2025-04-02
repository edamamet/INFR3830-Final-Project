using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;
namespace _Project._Scripts {
    public class SettingsMenu : MonoBehaviour, IMenu {
        [SerializeField] CanvasGroup canvas;
        [SerializeField] Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider;
        ChannelGroup masterChannel;
        VCA musicChannel, sfxChannel;
        void Awake() {
            RuntimeManager.CoreSystem.getMasterChannelGroup(out masterChannel);
            musicChannel = RuntimeManager.GetVCA("vca:/Music");
            sfxChannel = RuntimeManager.GetVCA("vca:/SFX");
        }
        public void Show() {
            canvas.Show();
            
            masterChannel.getVolume(out float masterVolume);
            masterVolumeSlider.value = masterVolume;
            
            musicChannel.getVolume(out float musicVolume);
            musicVolumeSlider.value = musicVolume;
            
            sfxChannel.getVolume(out float sfxVolume);
            sfxVolumeSlider.value = sfxVolume;
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

        public void SetMasterVolume(float value) {
            masterChannel.setVolume(value);
        }

        public void SetMusicVolume(float value) {
            musicChannel.setVolume(value);
        }

        public void SetSfxVolume(float value) {
            sfxChannel.setVolume(value);
        }
    }
}
