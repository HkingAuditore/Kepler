using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LabModeUI : MonoBehaviour
{
    public void ToLab()
    {
        SceneManager.LoadScene("PhysicScene");
    }

    public void ToMoon()
    {
        SceneManager.LoadScene("Satellite");
    }

    public void ToMain()
    {
        SceneManager.LoadScene("Main");
    }
}
