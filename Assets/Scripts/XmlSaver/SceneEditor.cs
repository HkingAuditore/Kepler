using System;
using System.Collections;
using Quiz;
using SpacePhysic;
using UnityEngine;

namespace XmlSaver
{
    public class SceneEditor : SceneLoadBase<AstralBody>
    {
        public XmlSaver<AstralBody> saver;
        public void SaveQuiz(string quizName)
        {
            // orbitBase.Freeze(false);
            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict);
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

            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict);
            saver.SaveXml(xmlDoc, quizName);
        }

        /// <summary>
        ///     加入星体
        /// </summary>
        /// <param name="astralBody"></param>
        /// <param name="isTarget">是否为目标</param>
        public void AddAstralBody(AstralBody astralBody, bool isTarget = false)
        {
            astralBodiesDict.Add(new AstralBodyDict<AstralBody>(astralBody.transform, astralBody, isTarget));
        }

        /// <summary>
        ///     设置问题目标
        /// </summary>
        /// <param name="target"></param>
        public void SetCore(AstralBody core)
        {
            var oriCore= this.core;
            this.core = core;
            astralBodiesDict.ForEach(ast =>
                                     {
                                         ast.isCore = ast.astralBody == core;
                                         ast.astralBody.affectedPlanets.Remove(oriCore);
                                         if (!ast.isCore)
                                         {
                                             ast.astralBody.affectedPlanets.Remove(core);
                                             ast.astralBody.affectedPlanets.Add(core);
                                         }

                                         // Debug.Log(ast.astralBody.gameObject.name + " is target ? : " + ast.isTarget);
                                         // ast.isTarget = true;
                                     });
        }

    }
}