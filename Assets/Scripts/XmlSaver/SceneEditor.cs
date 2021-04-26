using System;
using System.Collections;
using System.Linq;
using Quiz;
using SpacePhysic;
using StaticClasses;
using UnityEngine;

namespace XmlSaver
{
    /// <summary>
    /// 场景编辑器
    /// </summary>
    public class SceneEditor : SceneLoadBase<AstralBody>
    {
        public SceneSaver saver;
        /// <summary>
        /// 保存场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void SaveScene(string sceneName)
        {
            // orbitBase.Freeze(false);
            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict);
            try
            {
                saver.SaveXml(xmlDoc, sceneName);
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
        /// 更新星体组
        /// </summary>
        public void UpdateAstralBody()
        {
            Debug.Log("before update:" + astralBodiesDict.Count);
            astralBodiesDict = astralBodiesDict.Where(a => a.astralBody.gameObject.CheckReference()).ToList();
            Debug.Log("after update:" + astralBodiesDict.Count);
        }

        /// <summary>
        /// 移除星体
        /// </summary>
        /// <param name="astralBody"></param>
        public void RemoveAstralBodyDict(AstralBody astralBody)
        {
            astralBodiesDict = astralBodiesDict.Where(a => a.astralBody!= astralBody ).ToList();
        }



        /// <summary>
        ///     设置场景核心
        /// </summary>
        /// <param name="core">新核心</param>
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