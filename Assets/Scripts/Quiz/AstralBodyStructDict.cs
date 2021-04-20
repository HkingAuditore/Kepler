using SpacePhysic;
using UnityEngine;

namespace Quiz
{
    public struct AstralBodyStructDict
    {
        /// <summary>
        ///     位置坐标
        /// </summary>
        public Vector3 position;

        /// <summary>
        ///     质量
        /// </summary>
        public double mass;

        /// <summary>
        ///     质量是否为条件
        /// </summary>
        public bool isMassPublic;

        /// <summary>
        ///     密度
        /// </summary>
        public double density;

        /// <summary>
        ///     星球半径
        /// </summary>
        public float originalSize;

        /// <summary>
        ///     引力影响范围
        /// </summary>
        public float affectRadius;

        /// <summary>
        ///     初速度
        /// </summary>
        public Vector3 oriVelocity;

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
        ///     周期
        /// </summary>
        public float period;

        /// <summary>
        ///     周期是否为条件
        /// </summary>
        public bool isPeriodPublic;

        /// <summary>
        ///     与中心星体的距离
        /// </summary>
        public float radius;

        /// <summary>
        ///     与中心星体的距离是否为条件
        /// </summary>
        public bool isRadiusPublic;

        /// <summary>
        ///     星球使用样式编号
        /// </summary>
        public int meshNum;

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
        ///     是否可以影响其他星球
        /// </summary>
        public bool enableAffect;

        /// <summary>
        ///     是否可以被追踪
        /// </summary>
        public bool enableTracing;

        /// <summary>
        ///     是否为目标
        /// </summary>
        public bool isTarget;

        /// <summary>
        ///     是否为中心天体
        /// </summary>
        public bool isCore;

        public AstralBodyStructDict(Transform transform, AstralBody astralBody, bool isTarget, bool isCore)
        {
            position      = transform.position;
            mass          = astralBody.Mass;
            density       = astralBody.density;
            originalSize  = astralBody.size;
            affectRadius  = astralBody.affectRadius;
            oriVelocity   = astralBody.oriVelocity;
            enableAffect  = astralBody.enableAffect;
            enableTracing = astralBody.enableTracing;
            meshNum       = astralBody.meshNum;
            this.isTarget = isTarget;
            this.isCore   = isCore;

            isMassPublic = false;
            // angularVelocity         = default;
            isAngularVelocityPublic = false;
            period                  = 0;
            isPeriodPublic          = false;
            radius                  = 0;
            isRadiusPublic          = false;
            // AnglePerT               = 0;
            // isAnglePerTPublic       = false;
            // distancePerT            = 0;
            // isDistancePerTPublic    = false;
            t                = 0;
            isTPublic        = false;
            isVelocityPublic = false;
            isGravityPublic  = false;
            isSizePublic     = false;
        }
    }
}