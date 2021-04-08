using System;
using Quiz;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class QuizUI : MonoBehaviour
{
    public QuizSolver quizSolver;
    public Slider     quizSlider;
    public Text       title;
    public QuizType   quizType;
    public Text       ansText;
    public Button     confirm;
    public Text       quizCondition;

    public AstralBody target;

    [SerializeField] private int _ansPos;

    [SerializeField] private int _gap;

    private void Start()
    {
        
    }

    public void Generate()
    {
        this.target        = quizSolver.target;
        switch (quizType)
        {
            case QuizType.Mass:
                title.text = "质量:";
                break;
            case QuizType.Density:
                title.text = "密度:";
                break;
            case QuizType.Gravity:
                title.text = "重力加速度:";
                break;
            case QuizType.Radius:
                title.text = "轨道半径:";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        quizCondition.text = quizSolver.GetQuizSentence();
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
                ansText.text = ConvertSliderValue2Ans(quizSlider.value).ToString("f2") + " kg/m3";
                target.ChangeMass(ConvertSliderValue2Ans(quizSlider.value) * Mathf.PI * Mathf.Pow(target.size,3)*4/3);
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
        var tmpAns = ConvertSliderValue2Ans(quizSlider.value);
        switch (quizType)
        {
            case QuizType.Mass:
                quizSolver.TmpAnswer = tmpAns;
                break;
            case QuizType.Density:
                throw new ArgumentOutOfRangeException();
                quizSolver.TmpAnswer = tmpAns;
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

        gameObject.SetActive(false);
    }

    private void GenerateAns()
    {
        _gap    = (int) Random.Range(0, quizSolver.answer);
        _gap    = (int) Mathf.Clamp(_gap, 0.1f *quizSolver.answer, 0.3f * quizSolver.answer);
        _ansPos = Random.Range(0, (int) (quizSolver.answer / _gap));
    }

    private float ConvertSliderValue2Ans(float quizSliderValue)
    {
        //TODO:干扰项设计没做
        return quizSolver.answer + (quizSliderValue - _ansPos) * _gap;
    }
    
}