﻿using System;
using System.Collections.Generic;
using GameManagers;
using Quiz;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI.Quiz
{
    public class ResultUI : MonoBehaviour
    {
        public  GameObject       panel;
        public  QuizStarsGroupUI quizStarsGroupUI;
        public  List<GameObject> quizUis;
        public  Text             resultText;
        private QuizSolver       _quizSolver;


        private void Start()
        {
            _quizSolver = (QuizSolver) GameManager.getGameManager.quizBase;
            _quizSolver.resultEvent.AddListener(ShowResult);
        }

        private void ShowResult()
        {
            quizUis.ForEach(q => q.SetActive(false));
            switch (_quizSolver.reason)
            {
                case Reason.Right:
                    resultText.text = "你答对了！";
                    quizStarsGroupUI.CalculateSuccessStars();
                    break;
                case Reason.NonCircleOrbit:
                    resultText.text            = "你的轨道不是圆形！";
                    quizStarsGroupUI.starCount = 0;
                    break;
                case Reason.Crash:
                    resultText.text            = "你的星球被撞毁了！";
                    quizStarsGroupUI.starCount = 0;
                    break;
                case Reason.Overtime:
                    resultText.text            = "你超时了！";
                    quizStarsGroupUI.starCount = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            panel.SetActive(true);
        }

        public void Restart()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene(SceneManager.GetActiveScene().name);
        }

        public void Back()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("TestMode");
        }
    }
}