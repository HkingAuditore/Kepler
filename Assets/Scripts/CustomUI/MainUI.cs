using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI
{
    public class MainUI : MonoBehaviour
    {
        public GameObject settingPanel;
        public Dropdown   fullscreenSetting;

        public void LoadLabScene()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("LabMode");
        }

        public void LoadTestMode()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("TestMode");
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void OpenSettingPanel()
        {
            settingPanel.SetActive(true);
        }

        public void SetFullScreen()
        {
            switch (fullscreenSetting.value)
            {
                case 0:
                    Screen.fullScreen = true;
                    break;
                case 1 :
                    Screen.fullScreen = false;

                    break;
                default:
                    break;
            }
        }
    }
}