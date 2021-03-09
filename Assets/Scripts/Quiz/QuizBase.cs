using System;
using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;

namespace Quiz
{
    public class QuizBase : MonoBehaviour
    {
        public bool                 isLoadByPrefab;
        public string               loadTarget;
        public List<AstralBodyDict> astralBodiesDict;
        public AstralBody           astralBodyPrefab;
        public Transform            quizRoot;
        public GravityTracing       orbitBase;
        public QuizType             quizType;
        public AstralBody           target;

        public  float                      answer;
        private List<AstralBodyStructDict> _astralBodyStructDictList;


        public bool IsLoadDone { private set; get; }


        private void Start()
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
            foreach (var pair in astralBodiesDict)
            {
                var target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, quizRoot);
                orbitBase.AddTracingTarget(target);
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isTarget) this.target = target;
            }
        }

        private void GenerateAstralBodiesWithoutPrefab()
        {
            foreach (var pair in _astralBodyStructDictList)
            {
                astralBodyPrefab.mass          = pair.mass;
                astralBodyPrefab.density       = pair.density;
                astralBodyPrefab.originalSize  = pair.originalSize;
                astralBodyPrefab.oriVelocity   = pair.oriVelocity;
                astralBodyPrefab.affectRadius  = pair.affectRadius;
                astralBodyPrefab.enableAffect  = pair.enableAffect;
                astralBodyPrefab.enableTracing = pair.enableTracing;
                var target =
                    Instantiate(astralBodyPrefab, pair.position, Quaternion.Euler(0, 0, 0), quizRoot);
                orbitBase.AddTracingTarget(target);
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isCore)
                    target.gameObject.name = "Core";
                if (pair.isTarget) this.target = target;
            }
        }

        


        public void LoadQuiz(string fileName)
        {
            var result = QuizSaver.ConvertXml2QuizBase(QuizSaver.LoadXml(fileName));
            _astralBodyStructDictList = result.astralBodyStructList;
            quizType                  = result.quizType;
        }
    }
}