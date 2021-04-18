using System;
using System.Collections.Generic;
using GameManagers;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Events;

namespace Quiz
{
    public class QuizBase : MonoBehaviour
    {
        public                     bool                 isLoadByPrefab;
        public                     string               loadTarget;
        [SerializeField] protected List<AstralBodyDict> astralBodiesDict;
        public                     QuizAstralBody       astralBodyPrefab;
        public                     Transform            quizRoot;
        public                     GravityTracing       orbitBase;
        public                     QuizType             quizType;

        public float      answer;
        public UnityEvent loadDoneEvent = new UnityEvent();

        [SerializeField] private QuizAstralBody _target;

        private List<AstralBodyStructDict> _astralBodyStructDictList;

        private bool _isLoadDone;

        public QuizAstralBody target
        {
            get => _target;
            protected set => _target = value;
        }


        public bool isLoadDone
        {
            private set
            {
                _isLoadDone = value;
                if (isLoadDone) loadDoneEvent.Invoke();
            }
            get => _isLoadDone;
        }


        public virtual void Start()
        {
            loadDoneEvent.AddListener(() => { GameManager.GetGameManager.CalculateMassScales(); });
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
                    if (!GameManager.GetGameManager.isQuizEditMode)
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


        private void GenerateAstralBodiesWithPrefab()
        {
            var astralBodyDicts = new List<AstralBodyDict>();
            foreach (var pair in astralBodiesDict)
            {
                var target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, quizRoot);
                orbitBase.AddTracingTarget(target);
                try
                {
                    if (!pair.isTarget)
                        target.affectedPlanets.Add(this.target);
                }
                catch (Exception e)
                {
                }

                astralBodyDicts.Add(new AstralBodyDict(target.transform, target, pair.isTarget));
                // Debug.Log("add HashCode:" + target.GetHashCode());
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isTarget) this.target = target;
            }

            astralBodiesDict = astralBodyDicts;
        }

        private void GenerateAstralBodiesWithoutPrefab()
        {
            var astralBodyDicts = new List<AstralBodyDict>();
            foreach (var pair in _astralBodyStructDictList)
            {
                astralBodyPrefab.realMass     = pair.mass;
                astralBodyPrefab.isMassPublic = pair.isMassPublic;

                // astralBodyPrefab.density      = pair.density;
                astralBodyPrefab.size = pair.originalSize;

                astralBodyPrefab.oriVelocity      = pair.oriVelocity;
                astralBodyPrefab.isVelocityPublic = pair.isVelocityPublic;

                astralBodyPrefab.affectRadius  = pair.affectRadius;
                astralBodyPrefab.enableAffect  = pair.enableAffect;
                astralBodyPrefab.enableTracing = pair.enableTracing;


                //  astralBodyPrefab.globalAngularVelocity = pair.angularVelocity;
                astralBodyPrefab.isAngularVelocityPublic = pair.isAngularVelocityPublic;
                //
                //
                astralBodyPrefab.radius         = pair.radius;
                astralBodyPrefab.isRadiusPublic = pair.isRadiusPublic;

                astralBodyPrefab.period         = pair.period;
                astralBodyPrefab.isPeriodPublic = pair.isPeriodPublic;


                astralBodyPrefab.isTPublic       = pair.isTPublic;
                astralBodyPrefab.t               = pair.t;
                astralBodyPrefab.isGravityPublic = pair.isGravityPublic;
                astralBodyPrefab.isSizePublic    = pair.isSizePublic;

                // astralBodyPrefab.anglePerT             = pair.AnglePerT;
                // astralBodyPrefab.isAnglePerTPublic     = pair.isAnglePerTPublic;


                // astralBodyPrefab.distancePerT     = pair.distancePerT;
                // astralBodyPrefab.isDistancePerTPublic = pair.isDistancePerTPublic;


                var target =
                    Instantiate(astralBodyPrefab, pair.position, Quaternion.Euler(0, 0, 0), quizRoot);
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
                astralBodyDicts.Add(new AstralBodyDict(target.transform, target, pair.isTarget));

                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isCore) target.gameObject.name = "Core";
                if (pair.isTarget) this.target          = target;
                target.UpdateQuizAstralBodyPer();
            }

            astralBodiesDict = astralBodyDicts;
        }


        public void LoadQuiz(string fileName)
        {
            var result = QuizSaver.ConvertXml2QuizBase(QuizSaver.LoadXml(fileName), fileName);
            _astralBodyStructDictList = result.astralBodyStructList;
            quizType                  = result.quizType;
        }
    }
}