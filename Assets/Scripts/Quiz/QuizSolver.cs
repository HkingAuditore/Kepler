using System.Collections;
using UnityEngine;

namespace Quiz
{
    public class QuizSolver : QuizBase
    {
        public  QuizUI quizUI;
        public  float  waitTime;
        private float  _tmpAnswer;

        public bool _isRight;
        public bool isRight
        {
            get => _isRight;
            set => _isRight = value;
        }

        public float TmpAnswer
        {
            get => _tmpAnswer;
            set
            {
                orbitBase.Freeze(false);
                _tmpAnswer = value;
                FinishQuiz(_tmpAnswer.Equals(answer));
            }
        }


        private void FinishQuiz(bool isRight)
        {
            WaitForAnswer(waitTime);
        }
        
        IEnumerator WaitForAnswer(float time) {
           
            yield return new WaitForSeconds(time);
            
        }

        public override void Start()
        {
            base.Start();
            this.quizUI.target = this.target;
        }
    }
}