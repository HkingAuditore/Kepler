using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    public class MainUI : MonoBehaviour
    {
        public GameObject settingPanel;

        public void LoadLabScene()
        {
            SceneManager.LoadScene("LabMode");
        }

        public void LoadTestMode()
        {
            SceneManager.LoadScene("TestMode");
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