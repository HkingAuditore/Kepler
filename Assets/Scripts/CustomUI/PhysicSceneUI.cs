using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI
{
    public class PhysicSceneUI : MonoBehaviour
    {
        public InputField sceneNameInputField;
        public GameObject sceneSettingPanel;
        public GameObject normalPanel;
        public Toggle     pauseToggle;
        public void BackToMain()
        {
            SceneManager.LoadScene("LabMode");
        }

        public void SaveScene()
        {
            GameManager.getGameManager.sceneEditor.SaveScene(sceneNameInputField.text);
        }

        public void ShowSceneSettingPanel()
        {
            sceneSettingPanel.SetActive(true);
            normalPanel.SetActive(false);
        }

        public void CloseSceneSettingPanel()
        {
            sceneSettingPanel.SetActive(false);
            normalPanel.SetActive(true);
        }

        public void SetTimePause()
        {
            GameManager.getGameManager.orbit.Freeze(pauseToggle.isOn);
        }
    }
}