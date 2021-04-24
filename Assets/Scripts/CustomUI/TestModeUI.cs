using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    public class TestModeUI : MonoBehaviour
    {
        public void Back()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("Main");
        }

        public void ToEditMode()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("QuizEdit");
        }
    }
}