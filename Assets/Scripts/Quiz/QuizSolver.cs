using System;
using System.Collections;
using System.Linq;
using System.Text;
using CustomUI.Quiz;
using GameManagers;
using UnityEngine;
using UnityEngine.Events;

namespace Quiz
{
    public class QuizSolver : QuizBase
    {
        /// <summary>
        ///     回答事件
        /// </summary>
        public UnityEvent answerEvent;

        public QuizUI quizUI;

        /// <summary>
        ///     容错范围
        /// </summary>
        public float radiusOffset = .2f;

        /// <summary>
        ///     结果事件
        /// </summary>
        public UnityEvent resultEvent;

        /// <summary>
        ///     检查等待时间
        /// </summary>
        public float waitTime;

        private Reason _reason;
        private float  _tmpAnswer;

        /// <summary>
        ///     回答是否正确
        /// </summary>
        public bool isRight { get; set; } = true;

        public float tmpAnswer
        {
            get => _tmpAnswer;
            set
            {
                _tmpAnswer = value;
                FinishQuiz(_tmpAnswer.Equals(answer));
                answerEvent.Invoke();
            }
        }

        /// <summary>
        ///     结果
        /// </summary>
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

        protected override void Start()
        {
            base.Start();
            quizUI.quizType = quizType;

            GameManager.getGameManager.globalTimer.countingDownEndEvent.AddListener(() =>
            {
                reason = Reason.Overtime;
                resultEvent.Invoke();
            });
            GameManager.getGameManager.globalTimer.StartCounting();
            resultEvent.AddListener(() =>
                                        GameManager.getGameManager.globalTimer.isPausing = true
                                   );
            StartCoroutine(WaitUntilQuizAstralBodyLoadDone());

        }
        
        IEnumerator WaitUntilQuizAstralBodyLoadDone()
        {
            yield return new WaitUntil(() => astralBodiesDict.All(a => a.astralBody.isLoadDone));
            quizUI.Generate();
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

        /// <summary>
        ///     获取问题
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string GetQuizSentence()
        {
            var stringBuilder = new StringBuilder(" ");
            var centerString  = target.GetQuizConditionString();
            if (centerString != null) stringBuilder.Append("中心星体的" + centerString + "；");
            var i = 1;
            foreach (AstralBodyDict<QuizAstralBody> dict in astralBodiesDict)
            {
                if (dict.isTarget) continue;
                var orbitString = dict.astralBody.GetQuizConditionString();
                if (orbitString != null) stringBuilder.Append("绕转星体" + i + "的" + orbitString + "；");
                // Debug.Log("mass:" + dict.astralBody.realMass);
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