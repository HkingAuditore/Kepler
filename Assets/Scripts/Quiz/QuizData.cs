using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Quiz
{
    public class QuizData
    {
        public string quizName;
        public List<AstralBodyDict> astralBodies;
    }

    [Serializable]
    public class QuizSaverException : Exception
    {

        public QuizSaverException()
        {
        }

        public QuizSaverException(string message) : base(message)
        {
        }

        public QuizSaverException(string message, Exception inner) : base(message, inner)
        {
        }

        protected QuizSaverException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
