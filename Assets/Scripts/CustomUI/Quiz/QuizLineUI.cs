using System;
using GameManagers;
using Quiz;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI.Quiz
{
    public class QuizLineUI : MonoBehaviour
    {
        public string         name;
        public QuizListUI     quizListUI;
        public Text           quizName;
        public Text           quizType;
        public QuizBaseStruct quizStruct;

        private void OnEnable()
        {
            Generate();
        }

        public void OnClick()
        {
            GlobalTransfer.getGlobalTransfer.sceneName = name;
            SceneManager.LoadScene("QuizTest");
        }

        public void Delete()
        {
            quizListUI.DeleteQuiz(name);
        }

        private void Generate()
        {
            quizName.text = name;
            switch (quizStruct.quizType)
            {
                case QuizType.Mass:
                    quizType.text = "质量问题";
                    break;
                case QuizType.Density:
                    quizType.text = "密度问题";

                    break;
                case QuizType.Gravity:
                    quizType.text = "重力问题";

                    break;
                case QuizType.Radius:
                    quizType.text = "轨道问题";

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}