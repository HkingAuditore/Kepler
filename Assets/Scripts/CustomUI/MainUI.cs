using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    public class MainUI : MonoBehaviour
    {
        public GameObject settingPanel;

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
    }
}