using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    public class LabModeUI : MonoBehaviour
    {
        public GameObject difficultyPanel;

        public void ToLab()
        {
            SceneManager.LoadScene("PhysicScene");
        }

        public void ToMoon(int difficulty)
        {
            GlobalTransfer.getGlobalTransfer.difficulty = (Difficulty) difficulty;
            SceneManager.LoadScene("Satellite");
        }

        public void ToMain()
        {
            SceneManager.LoadScene("Main");
        }

        public void ShowDifficultyPanel()
        {
            difficultyPanel.SetActive(true);
        }

        public void CloseDifficultyPanel()
        {
            difficultyPanel.SetActive(false);
        }
    }
}