using System;
using System.Collections;
using System.Text;
using CustomUI.Quiz;
using GameManagers;
using UnityEngine;
using UnityEngine.Events;

namespace Quiz
{
    public enum Reason
    {
        Right,
        NonCircleOrbit,
        Crash,
        Overtime
    }

    public class QuizSolver : QuizBase
    {
        public QuizUI     quizUI;
        public float      waitTime;
        public float      radiusOffset = .2f;
        public UnityEvent resultEvent;
        public UnityEvent answerEvent;

        private Reason _reason;

        private float _tmpAnswer;

        public bool isRight { get; set; } = true;

        public float TmpAnswer
        {
            get => _tmpAnswer;
            set
            {
                _tmpAnswer = value;
                FinishQuiz(_tmpAnswer.Equals(answer));
                answerEvent.Invoke();
            }
        }

        public Reason reason
        {
            get => _reason;
            set
            {
                _reason = value;
                if (_reason == Reason.Crash)
                    resultEvent.Invoke();
            }
        }

        public bool isAnswered { get; set; } = false;

        public override void Start()
        {
            base.Start();
            quizUI.quizType = quizType;
            quizUI.Generate();
            GameManager.GetGameManager.globalTimer.countingDownEndEvent.AddListener(() =>
                                                                                    {
                                                                                        reason = Reason.Overtime;
                                                                                        resultEvent.Invoke();
                                                                                    });
            GameManager.GetGameManager.globalTimer.StartCounting();
            resultEvent.AddListener(() =>
                                        GameManager.GetGameManager.globalTimer.isPausing = true
                                   );
        }


        private void FinishQuiz(bool isRight)
        {
            astralBodiesDict.ForEach(pair =>
                                     {
                                         pair.astralBody.oriRadius =
                                             Vector3.Distance(pair.astralBody.transform.position,
                                                              target.transform.position);
                                         Debug.Log("Test Result Ori Radius:" + pair.astralBody.oriRadius);
                                     });
            orbitBase.Freeze(false);

            StartCoroutine(WaitForAnswer(waitTime));
        }

        private IEnumerator WaitForAnswer(float time)
        {
            yield return new WaitForSeconds(time);
            Debug.Log("Test Result:Time out");
            if (isRight)
            {
                Debug.Log("Test Result: Right!");
                reason = Reason.Right;
            }

            resultEvent.Invoke();
        }

        public string GetQuizSentence()
        {
            var stringBuilder = new StringBuilder(" ");
            var centerString  = target.GetQuizConditionString();
            if (centerString != null) stringBuilder.Append("中心星体的" + centerString + "；");
            var i = 1;
            foreach (var dict in astralBodiesDict)
            {
                if (dict.isTarget) continue;
                var orbitString = dict.astralBody.GetQuizConditionString();
                if (orbitString != null) stringBuilder.Append("绕转星体" + i + "的" + orbitString + "；");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append("。请求出");
            switch (quizType)
            {
                case QuizType.Mass:
                    stringBuilder.Append("中心天体的质量。");
                    break;
                case QuizType.Density:
                    stringBuilder.Append("中心天体的密度。");

                    break;
                case QuizType.Gravity:
                    stringBuilder.Append("中心天体的重力加速度。");

                    break;
                case QuizType.Radius:
                    stringBuilder.Append("轨道半径。");

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return stringBuilder.ToString();
        }
    }
}