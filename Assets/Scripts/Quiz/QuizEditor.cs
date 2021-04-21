using System;
using System.Collections;
using UnityEngine;

namespace Quiz
{
    public class QuizEditor : QuizBase
    {
        public QuizSaver saver;

        /// <summary>
        ///     保存问题
        /// </summary>
        /// <param name="quizName"></param>
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

        private IEnumerator WaitForCalculate(string quizName)
        {
            yield return new WaitForSeconds(2);

            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict, quizType);
            saver.SaveXml(xmlDoc, quizName);
        }

        /// <summary>
        ///     加入星体
        /// </summary>
        /// <param name="astralBody"></param>
        /// <param name="isTarget">是否为目标</param>
        public void AddAstralBody(QuizAstralBody astralBody, bool isTarget = false)
        {
            astralBodiesDict.Add(new AstralBodyDict<QuizAstralBody>(astralBody.transform, astralBody, isTarget));
        }

        /// <summary>
        ///     设置问题目标
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(QuizAstralBody target)
        {
            var oriTarget = this.target;
            this.target = target;
            astralBodiesDict.ForEach(ast =>
                                     {
                                         // Debug.Log(ast.astralBody.GetHashCode() + " <==> " + target.GetHashCode());
                                         ast.isTarget = ast.astralBody == target;
                                         ast.astralBody.affectedPlanets.Remove(oriTarget);
                                         if (!ast.isTarget)
                                         {
                                             ast.astralBody.affectedPlanets.Remove(target);
                                             ast.astralBody.affectedPlanets.Add(target);
                                         }

                                         // Debug.Log(ast.astralBody.gameObject.name + " is target ? : " + ast.isTarget);
                                         // ast.isTarget = true;
                                     });
        }
    }
}