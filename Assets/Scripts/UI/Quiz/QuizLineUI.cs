﻿using System;
using System.Collections;
using System.Collections.Generic;
using Quiz;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizLineUI : MonoBehaviour
{
    public  Text           quizName;
    public  Text           quizType;
    
    public string         name;
    public QuizBaseStruct quizStruct;
    public QuizListUI     quizListUI;

    private void OnEnable()
    {
        Generate();
    }

    public void OnClick()
    {
        GlobalTransfer.getGlobalTransfer.quizName = this.name;
        SceneManager.LoadScene("QuizTest");
    }

    public void Delete()
    {
        quizListUI.DeleteQuiz(this.name);
    }

    public void Generate()
    {
        this.quizName.text = name;
        switch (quizStruct.quizType)
        {
            case QuizType.Mass:
                this.quizType.text = "质量问题";
                break;
            case QuizType.Density:
                this.quizType.text = "密度问题";

                break;
            case QuizType.Gravity:
                this.quizType.text = "重力问题";

                break;
            case QuizType.Radius:
                this.quizType.text = "轨道问题";

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
}
