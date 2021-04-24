using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI.Quiz
{
    public class QuizTestUI : MonoBehaviour
    {
        public void BackToTestMode()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("TestMode");
        }
    }
}