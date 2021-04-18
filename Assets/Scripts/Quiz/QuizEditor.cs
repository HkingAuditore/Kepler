using System;
using System.Collections;
using UnityEngine;

namespace Quiz
{
    public class QuizEditor : QuizBase
    {
        public QuizSaver saver;
        
        public void SaveQuiz(string quizName)
        {
            // orbitBase.Freeze(false);
            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict, quizType);
            try
            {
                saver.SaveXml(xmlDoc, quizName);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            // GameManager.GetGameManager.CalculateScales();
            // StartCoroutine(WaitForCalculate(quizName));
        }
        
        IEnumerator WaitForCalculate(string quizName) {
            yield return new WaitForSeconds(2);
            
            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict, quizType);
            saver.SaveXml(xmlDoc, quizName);

        }

        
        public void AddAstralBody(QuizAstralBody astralBody, bool isTarget = false)
        {
            astralBodiesDict.Add(new AstralBodyDict(astralBody.transform, astralBody, isTarget));
        }

        public void SetTarget(QuizAstralBody target)
        {
            var oriTarget = this.target;
            this.target = (QuizAstralBody) target;
            astralBodiesDict.ForEach(ast =>
                                     {
                                         // Debug.Log(ast.astralBody.GetHashCode() + " <==> " + target.GetHashCode());
                                         ast.isTarget = (ast.astralBody == target);
                                         ast.astralBody.affectedPlanets.Remove(oriTarget);
                                         if(!ast.isTarget)
                                         {
                                             ast.astralBody.affectedPlanets.Remove(target);
                                             ast.astralBody.affectedPlanets.Add(target);
                                         }
                                         // Debug.Log(ast.astralBody.gameObject.name + " is target ? : " + ast.isTarget);
                                         // ast.isTarget = true;
                                     });
        }

        public void SetType(QuizType t)
        {
            quizType = t;
        }


    }
}
