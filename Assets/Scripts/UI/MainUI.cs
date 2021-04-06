using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    public void LoadLabScene()
    {
        SceneManager.LoadScene("PhysicScene") ;
    }

    public void LoadTestMode()
    {
        SceneManager.LoadScene("TestMode") ;

    }

    public void Quit()
    {
        Application.Quit();
    }
}
