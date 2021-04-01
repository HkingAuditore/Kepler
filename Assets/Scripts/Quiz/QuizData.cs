using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Quiz
{
    [Serializable]
    public struct AstralBodyDict
    {
        public Transform      transform;
        public QuizAstralBody astralBody;
        public bool           isTarget;

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
        public bool    isMassPublic;
        public float   density;
        public float   originalSize;
        public float   affectRadius;
        public Vector3 oriVelocity;
        public bool    isVelocityPublic;
        public float angularVelocity;
        public bool    isAngularVelocityPublic;
        public float   period;
        public bool    isPeriodPublic;
        public float   radius;
        public bool    isRadiusPublic;
        public float   AnglePerT;
        public bool    isAnglePerTPublic;
        public float   distancePerT;
        public bool    isDistancePerTPublic;
        public float   t;
        public bool    isTPublic;
        public bool    enableAffect;
        public bool    enableTracing;
        public bool    isTarget;
        public bool    isCore;

        public AstralBodyStructDict(Transform transform, AstralBody astralBody, bool isTarget, bool isCore)
        {
            position      = transform.position;
            mass          = astralBody.mass;
            density       = astralBody.density;
            originalSize  = astralBody.size;
            affectRadius  = astralBody.affectRadius;
            oriVelocity   = astralBody.oriVelocity;
            enableAffect  = astralBody.enableAffect;
            enableTracing = astralBody.enableTracing;
            this.isTarget = isTarget;
            this.isCore   = isCore;

            isMassPublic            = false;
            angularVelocity         = default;
            isAngularVelocityPublic = false;
            period                  = 0;
            isPeriodPublic          = false;
            radius                  = 0;
            isRadiusPublic          = false;
            AnglePerT               = 0;
            isAnglePerTPublic       = false;
            distancePerT            = 0;
            isDistancePerTPublic    = false;
            t                       = 0;
            isTPublic               = false;
            isVelocityPublic        = false;
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