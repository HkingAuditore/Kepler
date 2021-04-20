using System;
using GameManagers;
using MathPlus;
using Quiz;
using SpacePhysic;
using StaticClasses;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace CustomUI.Quiz
{
    public class QuizUI : MonoBehaviour
    {
        [SerializeField] private int _ansPos;

        [SerializeField] private int        _gap;
        public                   Text       ansText;
        public                   Button     confirm;
        public                   Text       quizCondition;
        public                   Slider     quizSlider;
        public                   QuizSolver quizSolver;
        public                   QuizType   quizType;

        public AstralBody target;
        public Text       title;


        public void Generate()
        {
            target = quizSolver.target;
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

                    target.realMass = ConvertSliderValue2Ans(quizSlider.value) *
                                      Mathf.Pow(10, -GameManager.getGameManager.globalMassScaler * 2);
                    ansText.text = target.realMass.GetMantissa().ToString("f2") + "x10e" +
                                   target.realMass.GetExponent().ToString().ToSuperscript();
                    break;
                case QuizType.Density:

                    target.realMass = ConvertSliderValue2Ans(quizSlider.value) * Mathf.PI *
                                      Mathf.Pow(
                                                target.size *
                                                Mathf.Pow(10, GameManager.getGameManager.GetK(PropertyUnit.M)),
                                                3) * 4 /
                                      3;
                    ansText.text = target.density.ToString("f2") + " kg/m3";
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
                    quizSolver.tmpAnswer = tmpAns;
                    break;
                case QuizType.Density:
                    // throw new ArgumentOutOfRangeException();
                    quizSolver.tmpAnswer = tmpAns;
                    // quizSolver.TmpAnswer = tmpAns;
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
            GameManager.getGameManager.globalTimer.isPausing = true;
        }

        private void GenerateAns()
        {
            _gap    = (int) Random.Range(0, quizSolver.answer);
            _gap    = (int) Mathf.Clamp(_gap, 0.1f * quizSolver.answer, 0.3f * quizSolver.answer);
            _ansPos = Random.Range(0, (int) (quizSolver.answer / _gap));
        }

        private float ConvertSliderValue2Ans(float quizSliderValue)
        {
            //TODO:干扰项设计没做
            return quizSolver.answer + (quizSliderValue - _ansPos) * _gap;
        }
    }
}