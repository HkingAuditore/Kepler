using System;
using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;

namespace Quiz
{
    [Serializable]
    public struct AstralBodiesDict
    {
        public Transform transform;
        public AstralBody astralBody;
        public bool isTarget;
    }

    public enum QuizType
    {
        Mass,
        Density,
        Gravity,
        Radius
    }
    public class QuizBase : MonoBehaviour
    {

        public AstralBodiesDict[] astralBodiesDict;
        public Transform quizRoot;
        public GravityTracing orbitBase;
        public QuizType quizType;
        public QuizUI quizUI;
        

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

        private void Awake()
        {
            // List<AstralBody> astralBodies = new List<AstralBody>();
            // 放置星球
            foreach (AstralBodiesDict pair in astralBodiesDict)
            {
                AstralBody target = GameObject.Instantiate(pair.astralBody, pair.transform.position,pair.transform.rotation,quizRoot);
                orbitBase.AddTracingTarget(target);
                if (pair.isTarget) quizUI.target = target;
            }
            
            // Time.timeScale = 0f;
            orbitBase.DrawOrbits();
            orbitBase.Freeze(true);
            // Time.timeScale = 0f;
        }
        
        
        
    }
}
