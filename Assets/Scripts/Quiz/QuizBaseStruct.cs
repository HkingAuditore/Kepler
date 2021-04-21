using System.Collections.Generic;
using XmlSaver;

namespace Quiz
{
    public class QuizBaseStruct : SceneBaseStruct<QuizAstralBodyDict>
    {
        public string   quizName;
        public QuizType quizType;
    }
}