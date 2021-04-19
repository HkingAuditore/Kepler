using SpacePhysic;
using UnityEngine;

namespace Quiz
{
    public struct AstralBodyStructDict
    {
        public Vector3 position;
        public double  mass;
        public bool    isMassPublic;
        public double  density;
        public float   originalSize;
        public float   affectRadius;
        public Vector3 oriVelocity;

        public bool isVelocityPublic;

        // public float angularVelocity;
        public bool  isAngularVelocityPublic;
        public float period;
        public bool  isPeriodPublic;
        public float radius;
        public bool  isRadiusPublic;
        public int   meshNum;

        public bool isGravityPublic;

        public bool isSizePublic;

        // public float   AnglePerT;
        // public bool    isAnglePerTPublic;
        // public float   distancePerT;
        // public bool    isDistancePerTPublic;
        public float t;
        public bool  isTPublic;
        public bool  enableAffect;
        public bool  enableTracing;
        public bool  isTarget;
        public bool  isCore;

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