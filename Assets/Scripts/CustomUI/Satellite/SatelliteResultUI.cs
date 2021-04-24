using System;
using System.Collections;
using CustomUI.Quiz;
using GameManagers;
using Satellite;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI.Satellite
{
    public class SatelliteResultUI : MonoBehaviour
    {
        public GameObject       panel;
        public QuizStarsGroupUI quizStarsGroupUI;
        public Text             resultText;

        private void Start()
        {
            StartCoroutine(WaitForCheck());
        }

        private IEnumerator WaitForCheck()
        {
            yield return
                new WaitUntil(() => GameManager.getGameManager.satelliteChallengeManger.isCheckEnd); //Lambda表达式
            ShowResultPanel(GameManager.getGameManager.satelliteChallengeManger.satelliteResultType);
        }

        private void ShowResultPanel(SatelliteResultType resultType)
        {
            switch (resultType)
            {
                case SatelliteResultType.Success:
                    resultText.text = "\"这是我个人的一小步，却是人类的一大步。\"";
                    quizStarsGroupUI.CalculateSuccessStars();
                    break;
                case SatelliteResultType.Crash:
                    resultText.text            = "你的卫星成为了人类的新遗迹";
                    quizStarsGroupUI.starCount = 0;
                    break;
                case SatelliteResultType.NotOrbit:
                    resultText.text            = "未能进入目标轨道";
                    quizStarsGroupUI.starCount = 0;
                    break;
                case SatelliteResultType.NonResult:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
            }

            panel.gameObject.SetActive(true);
        }

        public void BackToLabMode()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("LabMode");
        }

        public void Reload()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("Satellite");
        }
    }
}