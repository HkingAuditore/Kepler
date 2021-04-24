using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI.Quiz
{
    public class QuizEditUI : MonoBehaviour
    {
        public void BackToTestMode()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("TestMode");
        }
    }
}