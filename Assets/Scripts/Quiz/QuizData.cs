using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Quiz
{
    [Serializable]
    public struct AstralBodyDict
    {
        public Transform  transform;
        public QuizAstralBody astralBody;
        public bool       isTarget;

        public AstralBodyDict(Transform transform, QuizAstralBody astralBody, bool isTarget)
        {
            this.transform  = transform;
            this.astralBody = astralBody;
            this.isTarget   = isTarget;
        }
    }

    public struct AstralBodyStructDict
    {
        public Vector3 position;
        public float   mass;
        public float   density;
        public float   originalSize;
        public float   affectRadius;
        public Vector3 oriVelocity;
        public bool    enableAffect;
        public bool    enableTracing;
        public bool    isTarget;
        public bool    isCore;

        public AstralBodyStructDict(Transform transform, AstralBody astralBody, bool isTarget, bool isCore)
        {
            position      = transform.position;
            mass          = astralBody.mass;
            density       = astralBody.density;
            originalSize  = astralBody.originalSize;
            affectRadius  = astralBody.affectRadius;
            oriVelocity   = astralBody.oriVelocity;
            enableAffect  = astralBody.enableAffect;
            enableTracing = astralBody.enableTracing;
            this.isTarget = isTarget;
            this.isCore   = isCore;
        }
    }

    public enum QuizType
    {
        Mass,
        Density,
        Gravity,
        Radius
    }

    public class QuizData
    {
        public List<AstralBodyDict> astralBodies;
        public string               quizName;
    }

    [Serializable]
    public class QuizSaverException : Exception
    {
        public QuizSaverException()
        {
        }

        public QuizSaverException(string message) : base(message)
        {
        }

        public QuizSaverException(string message, Exception inner) : base(message, inner)
        {
        }

        protected QuizSaverException(
            SerializationInfo info,
            StreamingContext  context) : base(info, context)
        {
        }
    }
}