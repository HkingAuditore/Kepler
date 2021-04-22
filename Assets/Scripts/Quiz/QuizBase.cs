using System;
using System.Collections.Generic;
using GameManagers;
using UnityEngine;
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
        protected override void Start()
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
                        loadTarget = GlobalTransfer.getGlobalTransfer.sceneName;
                }
                catch (Exception e)
                {
                    // ignored
                }

                LoadScene(loadTarget);
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

            orbitBase.Freeze(true);
            isLoadDone = true;
        }


        protected override void GenerateAstralBodiesWithPrefab()
        {
            var astralBodyDicts = new List<QuizAstralBodyDict>();
            foreach (var astralBodyDict in astralBodiesDict)
            {
                var pair = (QuizAstralBodyDict) astralBodyDict;
                var target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, OrbitRoot);
                orbitBase.AddTracingTarget(target);
                try
                {
                    if (!pair.isTarget)
                        target.affectedPlanets.Add(this.target);
                }
                catch (Exception e)
                {
                }

                astralBodyDicts.Add(new QuizAstralBodyDict(target.transform, target, pair.isCore,pair.isTarget));
                // Debug.Log("add HashCode:" + target.GetHashCode());
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isTarget) this.target = target;
            }

            astralBodiesDict = astralBodyDicts.ConvertAll(dict => (AstralBodyDict<QuizAstralBody>) dict);
        }

        protected override void GenerateAstralBodiesWithoutPrefab(
            AstralBodyDataDictProcessHandler dataDictProcessHandler  = null,
            AstralBodyDataDictProcessHandler afterDictProcessHandler = null)
        {
            base.GenerateAstralBodiesWithoutPrefab(dataDictProcessHandler ?? ((prefab, pair, dictList) =>
                                                                              {
                                                                                  var quizPair = (QuizAstralBodyDataDict) pair;
                                                                                  prefab.isMassPublic            = quizPair.isMassPublic;
                                                                                  prefab.isVelocityPublic        = quizPair.isVelocityPublic;
                                                                                  prefab.isAngularVelocityPublic = quizPair.isAngularVelocityPublic;
                                                                                  prefab.isRadiusPublic          = quizPair.isRadiusPublic;
                                                                                  prefab.isPeriodPublic          = quizPair.isPeriodPublic;
                                                                                  prefab.isTPublic               = quizPair.isTPublic;
                                                                                  prefab.t                       = quizPair.t;
                                                                                  prefab.isGravityPublic         = quizPair.isGravityPublic;
                                                                                  prefab.isSizePublic            = quizPair.isSizePublic;
                                                                              }), afterDictProcessHandler ??
                                                                                  ((prefab, pair, dictList) =>
                                                                                   {
                                                                                       var quizPair =
                                                                                           (QuizAstralBodyDataDict)
                                                                                           pair;
                                                                                       var target =
                                                                                           Instantiate(astralBodyPrefab,
                                                                                                       pair.position,
                                                                                                       Quaternion
                                                                                                          .Euler(0, 0,
                                                                                                                 0),
                                                                                                       OrbitRoot);
                                                                                       try
                                                                                       {
                                                                                           if (!quizPair.isTarget)
                                                                                               target.affectedPlanets
                                                                                                     .Add(this.target);
                                                                                       }
                                                                                       catch (Exception e)
                                                                                       {
                                                                                           // ignored
                                                                                       }

                                                                                       target.meshNum = pair.meshNum;
                                                                                       orbitBase
                                                                                          .AddTracingTarget(target);


                                                                                       // Debug.Log("add HashCode:" + target.GetHashCode());
                                                                                       dictList
                                                                                          .Add(new
                                                                                                   QuizAstralBodyDict(target.transform,
                                                                                                                      target,
                                                                                                                      quizPair
                                                                                                                         .isCore,
                                                                                                                      quizPair
                                                                                                                         .isTarget));

                                                                                       target.gameObject.name =
                                                                                           target.gameObject.name
                                                                                                 .Replace("(Clone)",
                                                                                                          "");
                                                                                       if (quizPair.isCore)
                                                                                           target.gameObject.name =
                                                                                               "Core";
                                                                                       if (quizPair.isTarget)
                                                                                           this.target = target;
                                                                                       target.UpdateQuizAstralBodyPer();
                                                                                   }));
        }


        protected override void LoadScene(string fileName)
        {
            var result = QuizSaver.ConvertXml2SceneBase(QuizSaver.LoadXml(fileName), fileName);
            _astralBodyStructDictList = result.astralBodyStructList;
            quizType                  = result.quizType;
        }
    }
}