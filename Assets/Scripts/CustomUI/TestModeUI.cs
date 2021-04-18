using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    public class TestModeUI : MonoBehaviour
    {
        public void Back()
        {
            SceneManager.LoadScene("Main");
        }

        public void ToEditMode()
        {
            SceneManager.LoadScene("QuizEdit");
        }
    }
}