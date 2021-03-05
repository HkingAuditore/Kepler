﻿using System;
using System.Collections;
using System.Collections.Generic;
using Quiz;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class QuizUI : MonoBehaviour
{
    public QuizBase quizBase;
    public Slider quizSlider;
    public Text title;
    public QuizType quizType;
    public Text ansText;
    public Button confirm;

    public AstralBody target;

    private void Start()
    {
        title.text = Enum.GetName(quizType.GetType(), quizType) + ":";
        GenerateAns();
    }

    public void OnValueChange()
    {
        switch (quizType)
        {
            case QuizType.Mass:
                ansText.text = ConvertSliderValue2Ans(quizSlider.value).ToString("f2") + " kg";
                target.ChangeMass(ConvertSliderValue2Ans(quizSlider.value));
                break;
            case QuizType.Density:
                break;
            case QuizType.Gravity:
                break;
            case QuizType.Radius:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }
    public void Confirm()
    {
        float tmpAns = ConvertSliderValue2Ans(quizSlider.value);
        switch (quizType)
        {
            case QuizType.Mass:
                quizBase.TmpAnswer = tmpAns;
                break;
            case QuizType.Density:
                throw new ArgumentOutOfRangeException();
                break;
            case QuizType.Gravity:
                throw new ArgumentOutOfRangeException();
                break;
            case QuizType.Radius:
                throw new ArgumentOutOfRangeException();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        this.gameObject.SetActive(false);
    }
    [SerializeField]
    private int _ansPos;
    [SerializeField]
    private int _gap;
    private void GenerateAns()
    {
        _gap = (int)Random.Range(0, quizBase.answer);
        _gap = Mathf.Clamp(_gap, 10, 1000);
        _ansPos = (int)Random.Range(0, (int) (quizBase.answer / _gap));
        
    }
    private float ConvertSliderValue2Ans(float quizSliderValue)
    {
        //TODO:干扰项设计没做
        return quizBase.answer  + (quizSliderValue - _ansPos) * _gap;
    }
}
