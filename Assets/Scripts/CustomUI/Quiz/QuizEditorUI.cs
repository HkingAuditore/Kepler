using System;
using GameManagers;
using Quiz;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI.Quiz
{
    public class QuizEditorUI : MonoBehaviour
    {
        public  GameObject mainPanel;
        public  InputField nameField;
        public  GameObject prop;
        public  GameObject quizSetting;
        private QuizEditor _quizEditor;

        private void Start()
        {
            _quizEditor = (QuizEditor) GameManager.getGameManager.quizBase;
        }

        public void SaveQuiz()
        {
            var quizEditor = (QuizEditor) GameManager.getGameManager.quizBase;
            switch (quizEditor.quizType)
            {
                case QuizType.Mass:
                    quizEditor.answer = quizEditor.target.Mass;
                    break;
                case QuizType.Density:
                    quizEditor.answer = (float) quizEditor.target.density;
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
            var t = QuizType.Mass;
            switch (type)
            {
                case 0:
                    t = QuizType.Mass;
                    break;
                case 1:
                    t = QuizType.Density;
                    break;
            }

            _quizEditor.quizType = t;
        }

        private void SettingToProp()
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
}