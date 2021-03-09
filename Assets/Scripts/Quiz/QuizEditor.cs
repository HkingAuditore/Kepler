using System;

namespace Quiz
{
    public class QuizEditor : QuizBase
    {
        public QuizSaver saver;
        
        public void SaveQuiz()
        {
            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict, quizType);
            saver.SaveXml(xmlDoc, DateTime.Now.ToString("yy-MM-dd"));
        }
        
        public void AddAstralBody(AstralBody astralBody, bool isTarget = false)
        {
            astralBodiesDict.Add(new AstralBodyDict(astralBody.transform, astralBody, isTarget));
        }

        public void SetTarget(AstralBody target)
        {
            astralBodiesDict.ForEach(ast => { ast.isTarget = ReferenceEquals(ast.astralBody, target); });
        }


    }
}
