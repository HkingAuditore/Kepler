using System.Collections.Generic;
using XmlSaver;

namespace Quiz
{
    public class QuizBaseStruct : SceneBaseStruct<QuizAstralBody>
    {
        public string   quizName;
        public QuizType quizType;
    }
}