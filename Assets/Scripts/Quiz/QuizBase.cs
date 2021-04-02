using System;
using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;

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

        public QuizAstralBody target
        {
            get => _target;
            protected set => _target = value;
        }

        public  float                      answer;
        private List<AstralBodyStructDict> _astralBodyStructDictList;
        [SerializeField]
        private QuizAstralBody             _target;


        public bool IsLoadDone { private set; get; }


        public virtual void Start()
        {
            // List<AstralBody> astralBodies = new List<AstralBody>();
            // 放置星球
            if (isLoadByPrefab)
            {
                GenerateAstralBodiesWithPrefab();
            }
            else
            {
                LoadQuiz(loadTarget);
                GenerateAstralBodiesWithoutPrefab();
            }

            orbitBase.DrawOrbits();
            orbitBase.Freeze(true);
            IsLoadDone = true;
        }


        private void GenerateAstralBodiesWithPrefab()
        {
            List<AstralBodyDict> astralBodyDicts = new List<AstralBodyDict>();
            foreach (var pair in astralBodiesDict)
            {
                QuizAstralBody target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, quizRoot);
                orbitBase.AddTracingTarget(target);
                try
                {
                    if(!pair.isTarget)
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
            this.astralBodiesDict = astralBodyDicts;
        }

        private void GenerateAstralBodiesWithoutPrefab()
        {
            List<AstralBodyDict> astralBodyDicts = new List<AstralBodyDict>();
            foreach (AstralBodyStructDict pair in _astralBodyStructDictList)
            {
                astralBodyPrefab.mass         = pair.mass;
                astralBodyPrefab.isMassPublic = pair.isMassPublic;
                
                astralBodyPrefab.density      = pair.density;
                astralBodyPrefab.size = pair.originalSize;
                
                astralBodyPrefab.oriVelocity      = pair.oriVelocity;
                astralBodyPrefab.isVelocityPublic = pair.isVelocityPublic;
                
                astralBodyPrefab.affectRadius     = pair.affectRadius;
                astralBodyPrefab.enableAffect     = pair.enableAffect;
                astralBodyPrefab.enableTracing    = pair.enableTracing;
                
                
                //  astralBodyPrefab.globalAngularVelocity = pair.angularVelocity;
                astralBodyPrefab.isAngularVelocityPublic = pair.isAngularVelocityPublic;
                //
                //
                astralBodyPrefab.radius                  = pair.radius;
                astralBodyPrefab.isRadiusPublic          = pair.isRadiusPublic;
                
                astralBodyPrefab.period       = pair.period;
                astralBodyPrefab.isPeriodPublic          = pair.isPeriodPublic;

                
                astralBodyPrefab.isTPublic       = pair.isTPublic;
                astralBodyPrefab.t               = pair.t;
                astralBodyPrefab.isGravityPublic = pair.isGravityPublic;
                astralBodyPrefab.isSizePublic    = pair.isSizePublic;
                
                // astralBodyPrefab.anglePerT             = pair.AnglePerT;
                // astralBodyPrefab.isAnglePerTPublic     = pair.isAnglePerTPublic;
               
                
                // astralBodyPrefab.distancePerT     = pair.distancePerT;
                // astralBodyPrefab.isDistancePerTPublic = pair.isDistancePerTPublic;
                
                
                QuizAstralBody target =
                    Instantiate(astralBodyPrefab, pair.position, Quaternion.Euler(0, 0, 0), quizRoot);
                target.meshNum = pair.meshNum;
                orbitBase.AddTracingTarget(target);
                try
                {
                    if(!pair.isTarget)
                        target.affectedPlanets.Add(this.target);
                }
                catch (Exception e)
                {
                    // ignored
                }

                // Debug.Log("add HashCode:" + target.GetHashCode());
                astralBodyDicts.Add(new AstralBodyDict(target.transform,target,pair.isTarget));
                
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isCore)
                {
                    target.gameObject.name = "Core";
                }
                if (pair.isTarget)
                {
                    this.target = target;
                }
                target.UpdateQuizAstralBodyPer();
                
            }

            this.astralBodiesDict = astralBodyDicts;
        }

        


        public void LoadQuiz(string fileName)
        {
            var result = QuizSaver.ConvertXml2QuizBase(QuizSaver.LoadXml(fileName));
            _astralBodyStructDictList = result.astralBodyStructList;
            quizType                  = result.quizType;
        }
    }
}