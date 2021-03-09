using System;
using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;
using Object = System.Object;

namespace Quiz
{
    public class QuizBase : MonoBehaviour
    {
        public bool isLoadByPrefab;
        public List<AstralBodyDict> astralBodiesDict;
        private List<AstralBodyStructDict> _astralBodyStructDictList;
        public AstralBody astralBodyPrefab;
        public Transform quizRoot;
        public GravityTracing orbitBase;
        public QuizType quizType;
        public QuizUI quizUI;
        public QuizSaver saver;

        public bool IsLoadDone { private set; get; } = false;

        public float answer;
        private float _tmpAnswer;

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

        private void FinishQuiz(bool isRight)
        {
            if (isRight)
            {
                return;
            }
            else
            {
                return;
            }
        }

        private void Start()
        {
            this.LoadQuiz("21-03-08");
            // List<AstralBody> astralBodies = new List<AstralBody>();
            // 放置星球
            if(isLoadByPrefab)
                GenerateAstralBodiesWithPrefab();
            else
                GenerateAstralBodiesWithoutPrefab();
            orbitBase.DrawOrbits();
            orbitBase.Freeze(true);
            IsLoadDone = true;
        }

        private void GenerateAstralBodiesWithPrefab()
        {
            foreach (AstralBodyDict pair in astralBodiesDict)
            {
                AstralBody target =
                    GameObject.Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, quizRoot);
                orbitBase.AddTracingTarget(target);
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isTarget)
                {
                    quizUI.target = target;
                }
            }
        }

        private void GenerateAstralBodiesWithoutPrefab()
        {
            foreach (AstralBodyStructDict pair in _astralBodyStructDictList)
            {
                astralBodyPrefab.mass = pair.mass;
                astralBodyPrefab.density = pair.density;
                astralBodyPrefab.originalSize = pair.originalSize;
                astralBodyPrefab.oriVelocity = pair.oriVelocity;
                astralBodyPrefab.enableAffect = pair.enableAffect;
                astralBodyPrefab.enableTracing = pair.enableTracing;
                AstralBody target =
                    GameObject.Instantiate(astralBodyPrefab, pair.position, Quaternion.Euler(0,0,0), quizRoot);
                orbitBase.AddTracingTarget(target);
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                if (pair.isCore)
                    target.gameObject.name = "Core";
                if (pair.isTarget)
                {
                    quizUI.target = target;
                }
            }
        }


        public void AddAstralBody(AstralBody astralBody,bool isTarget = false)
        {
            astralBodiesDict.Add(new AstralBodyDict(astralBody.transform,astralBody,isTarget));
        }

        public void SetTarget(AstralBody target)
        {
            astralBodiesDict.ForEach(ast =>
                                     {
                                         ast.isTarget = (Object.ReferenceEquals(ast.astralBody, target));
                                     });
        }

        public void SaveQuiz()
        {
            var xmlDoc = saver.ConvertOrbit2Xml(this.astralBodiesDict,this.quizType);
            saver.SaveXml(xmlDoc,DateTime.Now.ToString("yy-MM-dd"));
        }

        public void LoadQuiz(string fileName)
        {
            QuizBaseStruct result = saver.ConvertXml2QuizBase(saver.LoadXml(fileName));
            this._astralBodyStructDictList = result.astralBodyStructList;
            this.quizType = result.quizType;
        }


    }
}
