using System;
using System.Collections;
using UnityEngine;

namespace Quiz
{
    public class QuizEditor : QuizBase
    {
        public QuizSaver saver;
        
        public void SaveQuiz()
        {
            orbitBase.Freeze(false);
            StartCoroutine(WaitForCalculate());
        }
        
        IEnumerator WaitForCalculate() {
            yield return new WaitForSeconds(2);
            
            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict, quizType);
            saver.SaveXml(xmlDoc, DateTime.Now.ToString("yy-MM-dd"));

        }

        
        public void AddAstralBody(QuizAstralBody astralBody, bool isTarget = false)
        {
            astralBodiesDict.Add(new AstralBodyDict(astralBody.transform, astralBody, isTarget));
        }

        public void SetTarget(AstralBody target)
        {
            astralBodiesDict.ForEach(ast => { ast.isTarget = ReferenceEquals(ast.astralBody, target); });
        }

        public void SetType(QuizType t)
        {
            quizType = t;
        }


    }
}
