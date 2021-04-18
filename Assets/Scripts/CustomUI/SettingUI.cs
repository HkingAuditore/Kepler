using GameManagers;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class SettingUI : MonoBehaviour
    {
        public Slider      audioSlider;
        public AudioSource bgmSource;

        private void Start()
        {
            audioSlider.value = Mathf.Round(bgmSource.volume * 10);
        }

        public void ChangeAudioVolume()
        {
            GlobalTransfer.getGlobalTransfer.audioVolume = audioSlider.value * .1f;
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}