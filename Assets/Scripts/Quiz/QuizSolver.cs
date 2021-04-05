using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Quiz
{
    public enum Reason
    {
        Right,
        NonCircleOrbit,
        Crash,
    }
    public class QuizSolver : QuizBase
    {
        public QuizUI     quizUI;
        public float      waitTime;
        public float      radiusOffset = .2f;
        public UnityEvent resultEvent;
        public UnityEvent answerEvent;
        
        private float _tmpAnswer;
        private bool  _isRight = true;
        private bool  _isAnswered = false;
        public bool isRight
        {
            get => _isRight;
            set => _isRight = value;
        }

        private Reason _reason;

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
                if(_reason == Reason.Crash)
                    resultEvent.Invoke();
            }
        }

        public bool isAnswered
        {
            get => _isAnswered;
            set
            {
                _isAnswered = value;
                
            }
        }


        private void FinishQuiz(bool isRight)
        {

            astralBodiesDict.ForEach(pair =>
                                     {
                                         pair.astralBody.oriRadius = Vector3.Distance(pair.astralBody.transform.position, this.target.transform.position);
                                         Debug.Log("Test Result Ori Radius:" + pair.astralBody.oriRadius);

                                     });
            orbitBase.Freeze(false);

            StartCoroutine(WaitForAnswer(waitTime));
        }
        
        IEnumerator WaitForAnswer(float time) {
            yield return new WaitForSeconds(time);
            Debug.Log("Test Result:Time out");
            if (this.isRight)
            {
                Debug.Log("Test Result: Right!");
                this.reason = Reason.Right;
            }
            resultEvent.Invoke();
        }

        public override void Start()
        {
            base.Start();
            quizUI.Generate();
        }

        public String GetQuizSentence()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("中心星体的" + this.target.GetQuizConditionString() + "；");
            int i = 1;
            foreach (AstralBodyDict dict in astralBodiesDict)
            {
                if(dict.isTarget)continue;
                stringBuilder.Append("绕转星体" + i + "的" + dict.astralBody.GetQuizConditionString() + "；");
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