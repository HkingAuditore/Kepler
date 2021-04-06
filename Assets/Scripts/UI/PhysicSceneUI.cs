using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysicSceneUI : MonoBehaviour
{
    public void BackToMain()
    {
        SceneManager.LoadScene("Main");
    }
}
