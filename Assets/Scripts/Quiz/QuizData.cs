using System.Collections.Generic;

namespace Quiz
{
    /// <summary>
    /// 问题信息
    /// </summary>
    public class QuizData
    {
        public List<AstralBodyDict<QuizAstralBody>> astralBodies;
        public string                               quizName;
    }
}