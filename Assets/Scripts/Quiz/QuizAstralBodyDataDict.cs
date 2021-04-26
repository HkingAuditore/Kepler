using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;
using XmlSaver;

namespace Quiz
{ 
    /// <summary>
    /// 问题星体数据存储
    /// </summary>
    [SerializeField]
    public class QuizAstralBodyDataDict : XmlSaver.AstralBodyDataDict<QuizAstralBody>
    {

        /// <summary>
        ///     质量是否为条件
        /// </summary>
        public bool isMassPublic;

        /// <summary>
        ///     速度是否为条件
        /// </summary>
        public bool isVelocityPublic;

        // public float angularVelocity;
        /// <summary>
        ///     角速度是否为条件
        /// </summary>
        public bool isAngularVelocityPublic;


        /// <summary>
        ///     周期是否为条件
        /// </summary>
        public bool isPeriodPublic;


        /// <summary>
        ///     与中心星体的距离是否为条件
        /// </summary>
        public bool isRadiusPublic;


        /// <summary>
        ///     表面重力加速度是否为条件
        /// </summary>
        public bool isGravityPublic;

        /// <summary>
        ///     星球半径是否为条件
        /// </summary>
        public bool isSizePublic;

        // public float   AnglePerT;
        // public bool    isAnglePerTPublic;
        // public float   distancePerT;
        // public bool    isDistancePerTPublic;

        public float t;
        public bool  isTPublic;


        /// <summary>
        ///     是否为目标
        /// </summary>
        public bool isTarget;


        public QuizAstralBodyDataDict(Transform transform, QuizAstralBody astralBody, bool isTarget, bool isCore) : base(transform,astralBody,isCore)
        {

            isMassPublic            = false;
            isAngularVelocityPublic = false;
            period                  = 0;
            isPeriodPublic          = false;
            radius                  = 0;
            isRadiusPublic          = false;
            t                       = 0;
            isTPublic               = false;
            isVelocityPublic        = false;
            isGravityPublic         = false;
            isSizePublic            = false;

            this.isTarget = isTarget;
        }

        public QuizAstralBodyDataDict(AstralBodyDataDict<QuizAstralBody> quizAstralBodyDict)
        {
            position      = quizAstralBodyDict.position;
            mass          = quizAstralBodyDict.mass;
            density       = quizAstralBodyDict.density;
            originalSize  = quizAstralBodyDict.originalSize;
            affectRadius  = quizAstralBodyDict.affectRadius;
            oriVelocity   = quizAstralBodyDict.oriVelocity;
            enableAffect  = quizAstralBodyDict.enableAffect;
            enableTracing = quizAstralBodyDict.enableTracing;
            meshNum       = quizAstralBodyDict.meshNum;
            this.isCore   = isCore;
            
            period                  = 0;
            radius                  = 0;
            isMassPublic            = false;
            isAngularVelocityPublic = false;
            period                  = 0;
            isPeriodPublic          = false;
            radius                  = 0;
            isRadiusPublic          = false;
            t                       = 0;
            isTPublic               = false;
            isVelocityPublic        = false;
            isGravityPublic         = false;
            isSizePublic            = false;
        }

        public QuizAstralBodyDataDict()
        {
            
        }

        public static QuizAstralBodyDataDict FromAstralBodyDict(XmlSaver.AstralBodyDataDict<Quiz.QuizAstralBody> dataDict)
        {
            QuizAstralBodyDataDict quizAstralBodyDataDict = new QuizAstralBodyDataDict(dataDict);
            return quizAstralBodyDataDict;
        }
        
        
    }
}