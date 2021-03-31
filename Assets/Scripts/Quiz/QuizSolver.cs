using System.Collections;
using UnityEngine;

namespace Quiz
{
    public class QuizSolver : QuizBase
    {
        public  QuizUI quizUI;
        public  float  waitTime;
        public  float  radiusOffset = .2f;
        private float  _tmpAnswer;

        private bool _isRight = true;
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
                _tmpAnswer = value;
                FinishQuiz(_tmpAnswer.Equals(answer));
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
                
            }
        }

        public override void Start()
        {
            base.Start();
            this.quizUI.target = this.target;
        }
    }
}