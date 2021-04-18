using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI.Quiz
{
    public class QuizTestUI : MonoBehaviour
    {
        public void BackToTestMode()
        {
            SceneManager.LoadScene("TestMode");
        }
    }
}