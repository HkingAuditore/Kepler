using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    public class PhysicSceneUI : MonoBehaviour
    {
        public void BackToMain()
        {
            SceneManager.LoadScene("LabMode");
        }
    }
}