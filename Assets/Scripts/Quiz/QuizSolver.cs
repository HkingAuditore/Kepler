namespace Quiz
{
    public class QuizSolver : QuizBase
    {
        public  QuizUI quizUI;
        private float  _tmpAnswer;

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
            if (isRight)
                return;
        }
    }
}