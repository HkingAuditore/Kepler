using System;
using System.Collections;
using UnityEngine;

namespace Quiz
{
    /// <summary>
    /// 问题编辑器
    /// </summary>
    public class QuizEditor : QuizBase
    {
        public QuizSaver saver;

        protected override void Start()
        {
            base.Start();
            orbitBase.Dispatch();
        }

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
                                         QuizAstralBodyDict quizAstralBodyDict = (QuizAstralBodyDict) ast;
                                         // Debug.Log(ast.astralBody.GetHashCode() + " <==> " + target.GetHashCode());
                                         quizAstralBodyDict.isTarget = ast.astralBody == target;
                                         quizAstralBodyDict.astralBody.affectedPlanets.Remove(oriTarget);
                                         if (!quizAstralBodyDict.isTarget)
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