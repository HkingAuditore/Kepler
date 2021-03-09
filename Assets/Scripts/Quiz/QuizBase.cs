using System;
using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;

namespace Quiz
{
    public class QuizBase : MonoBehaviour
    {
        public bool                 isLoadByPrefab;
        public List<AstralBodyDict> astralBodiesDict;
        public AstralBody           astralBodyPrefab;
        public Transform            quizRoot;
        public GravityTracing       orbitBase;
        public QuizType             quizType;
        public QuizUI               quizUI;
        public QuizSaver            saver;

        public  float                      answer;
        private List<AstralBodyStructDict> _astralBodyStructDictList;
        private float                      _tmpAnswer;

        public bool IsLoadDone { private set; get; }

        public float TmpAnswer
        {
            get => _tmpAnswer;
            set
            {
                orbitBase.Freeze(false);
                _tmpAnswer = value;
                FinishQuiz(_tmpAnswer.Equals(answer));
            }
        }

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
                LoadQuiz("21-03-09");
                GenerateAstralBodiesWithoutPrefab();
            }

            orbitBase.DrawOrbits();
            orbitBase.Freeze(true);
            IsLoadDone = true;
        }

        private void FinishQuiz(bool isRight)
        {
            if (isRight)
                return;
        }

        private void GenerateAstralBodiesWithPrefab()
        {
            foreach (var pair in astralBodiesDict)
            {
                var target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, quizRoot);
                orbitBase.AddTracingTarget(target);
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isTarget) quizUI.target = target;
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
                if (pair.isTarget) quizUI.target = target;
            }
        }


        public void AddAstralBody(AstralBody astralBody, bool isTarget = false)
        {
            astralBodiesDict.Add(new AstralBodyDict(astralBody.transform, astralBody, isTarget));
        }

        public void SetTarget(AstralBody target)
        {
            astralBodiesDict.ForEach(ast => { ast.isTarget = ReferenceEquals(ast.astralBody, target); });
        }

        public void SaveQuiz()
        {
            var xmlDoc = saver.ConvertOrbit2Xml(astralBodiesDict, quizType);
            saver.SaveXml(xmlDoc, DateTime.Now.ToString("yy-MM-dd"));
        }

        public void LoadQuiz(string fileName)
        {
            var result = saver.ConvertXml2QuizBase(saver.LoadXml(fileName));
            _astralBodyStructDictList = result.astralBodyStructList;
            quizType                  = result.quizType;
        }
    }
}