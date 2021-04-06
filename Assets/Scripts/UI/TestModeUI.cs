using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestModeUI : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("Main") ;

    }

    public void ToEditMode()
    {
        SceneManager.LoadScene("QuizEdit");
    }
}
