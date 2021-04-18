using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI.Quiz
{
    public class QuizEditUI : MonoBehaviour
    {
        public void BackToTestMode()
        {
            SceneManager.LoadScene("TestMode");
        }
    }
}