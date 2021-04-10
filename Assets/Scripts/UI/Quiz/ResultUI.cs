using System;
using System.Collections;
using System.Collections.Generic;
using Quiz;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    public List<GameObject> quizUis;
    public Text             resultText;
    public GameObject       panel;

    private QuizSolver _quizSolver;
    

    private void Start()
    {
        _quizSolver             =  (QuizSolver)GameManager.GetGameManager.quizBase;
        _quizSolver.resultEvent.AddListener(ShowResult);

    }

    private void ShowResult()
    {
        quizUis.ForEach(q => q.SetActive(false));
        switch (_quizSolver.reason)
        {
            case Reason.Right:
                resultText.text = "你答对了！";
                break;
            case Reason.NonCircleOrbit:
                resultText.text = "你的轨道不是圆形！";
                break;
            case Reason.Crash:
                resultText.text = "你的星球被撞毁了！";
                break;
            case Reason.Overtime:
                resultText.text = "你超时了！";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        panel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Back()
    {
        SceneManager.LoadScene("TestMode");
    }
}
