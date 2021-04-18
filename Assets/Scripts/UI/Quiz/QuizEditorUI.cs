using System;
using System.Collections;
using System.Collections.Generic;
using Quiz;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class QuizEditorUI : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject quizSetting;
    public GameObject prop;
    public InputField nameField;

    private QuizEditor _quizEditor;

    private void Start()
    {
        this._quizEditor = (QuizEditor)GameManager.GetGameManager.quizBase;
    }

    public void SaveQuiz()
    {
        var quizEditor = ((QuizEditor) (GameManager.GetGameManager.quizBase));
        switch (quizEditor.quizType)
        {
            case QuizType.Mass:
                quizEditor.answer = quizEditor.target.Mass;
                break;
            case QuizType.Density:
                quizEditor.answer = (float)quizEditor.target.density;
                break;
            case QuizType.Gravity:
                break;
            case QuizType.Radius:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        quizEditor.SaveQuiz(nameField.text);
        SettingToProp();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChangeQuizType(int type)
    {
        QuizType t = QuizType.Mass;
        switch (type)
        {
            case 0:
                t = QuizType.Mass;
                break;
            case 1:
                t = QuizType.Density;
                break;
            default:
                break;
        }
        _quizEditor.quizType = t;
    }

    public void SettingToProp()
    {
        quizSetting.SetActive(false);
        prop.SetActive(true);
    }

    public void MainToSetting()
    {
        mainPanel.SetActive(false);
        quizSetting.SetActive(true);
    }

    public void SettingToMain()
    {
        mainPanel.SetActive(true);
        quizSetting.SetActive(false);

    }

    public void ToTestMode()
    {
        SceneManager.LoadScene("TestMode");
    }

}
