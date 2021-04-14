using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizTestUI : MonoBehaviour
{
    public void BackToTestMode()
    {
        SceneManager.LoadScene("TestMode");
    }
}
