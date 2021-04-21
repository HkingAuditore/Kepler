using System;
using System.Collections.Generic;
using System.Linq;
using GameManagers;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Events;
using XmlSaver;

namespace Quiz
{
    public class QuizBase : SceneLoadBase<QuizAstralBody>
    {
        [SerializeField] private QuizAstralBody _target;

        /// <summary>
        ///     答案
        /// </summary>
        public float answer;

        /// <summary>
        ///     问题类型
        /// </summary>
        public QuizType quizType;




        /// <summary>
        ///     问题目标
        /// </summary>
        public QuizAstralBody target
        {
            get => _target;
            protected set => _target = value;
        }

        /// <summary>
        ///     是否加载完成
        /// </summary>


        protected virtual void Start()
        {
            loadDoneEvent.AddListener(() => { GameManager.getGameManager.CalculateMassScales(); });
            // List<AstralBody> astralBodies = new List<AstralBody>();
            // 放置星球
            if (isLoadByPrefab)
            {
                GenerateAstralBodiesWithPrefab();
            }
            else
            {
                try
                {
                    if (!GameManager.getGameManager.isQuizEditMode)
                        loadTarget = GlobalTransfer.getGlobalTransfer.quizName;
                }
                catch (Exception e)
                {
                    // ignored
                }

                LoadQuiz(loadTarget);
                GenerateAstralBodiesWithoutPrefab();
            }

            switch (quizType)
            {
                case QuizType.Mass:
                    answer = target.Mass;
                    break;
                case QuizType.Density:
                    answer = (float) target.density;
                    break;
                case QuizType.Gravity:
                    break;
                case QuizType.Radius:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // orbitBase.DrawOrbits();
            orbitBase.Freeze(true);

            isLoadDone = true;
        }


        protected override void GenerateAstralBodiesWithPrefab()
        {
            var        astralBodyDicts = new List<AstralBodyDict<QuizAstralBody>>();
            foreach (var pair in astralBodiesDict)
            {
                var target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, sceneRoot);
                orbitBase.AddTracingTarget(target);
                try
                {
                    if (!pair.isTarget)
                        target.affectedPlanets.Add(this.target);
                }
                catch (Exception e)
                {
                }

                astralBodyDicts.Add(new AstralBodyDict<QuizAstralBody>(target.transform, target, pair.isTarget));
                // Debug.Log("add HashCode:" + target.GetHashCode());
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isTarget) this.target = target;
            }

            astralBodiesDict = astralBodyDicts;
        }

        protected override void GenerateAstralBodiesWithoutPrefab()
        {
            base.GenerateAstralBodiesWithoutPrefab(((prefab, pair) =>
                                                    {
                                                        prefab.isMassPublic            = pair.isMassPublic;
                                                        prefab.isVelocityPublic        = pair.isVelocityPublic;
                                                        prefab.isAngularVelocityPublic = pair.isAngularVelocityPublic;
                                                        prefab.isRadiusPublic          = pair.isRadiusPublic;
                                                        prefab.isPeriodPublic          = pair.isPeriodPublic;
                                                        prefab.isTPublic               = pair.isTPublic;
                                                        prefab.t                       = pair.t;
                                                        prefab.isGravityPublic         = pair.isGravityPublic;
                                                        prefab.isSizePublic            = pair.isSizePublic;

                                                    }));
            foreach (QuizAstralBodyDataDict pair in _astralBodyStructDictList)
            {
                var target =
                    Instantiate(astralBodyPrefab, pair.position, Quaternion.Euler(0, 0, 0), sceneRoot);
                target.meshNum = pair.meshNum;
                orbitBase.AddTracingTarget(target);
                try
                {
                    if (!pair.isTarget)
                        target.affectedPlanets.Add(this.target);
                }
                catch (Exception e)
                {
                    // ignored
                }

                // Debug.Log("add HashCode:" + target.GetHashCode());
                astralBodyDicts.Add(new AstralBodyDict<QuizAstralBody>(target.transform, target, pair.isTarget));

                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isCore) target.gameObject.name = "Core";
                if (pair.isTarget) this.target          = target;
                target.UpdateQuizAstralBodyPer();

            }

            astralBodiesDict = astralBodyDicts;
        }


        private void LoadQuiz(string fileName)
        {
            var result  = QuizSaver.ConvertXml2SceneBase(QuizSaver.LoadXml(fileName), fileName);
            _astralBodyStructDictList =result.astralBodyStructList;
            quizType                  = result.quizType;
        }
    }
}