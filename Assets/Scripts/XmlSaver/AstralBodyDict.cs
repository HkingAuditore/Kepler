using SpacePhysic;
using UnityEngine;

namespace XmlSaver
{
    public interface IAstralBodyDictale
    {
        
    }
    public class AstralBodyDict<T> : IAstralBodyDictale where T : AstralBody
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
        ///     周期
        /// </summary>
        public float period;

        /// <summary>
        ///     与中心星体的距离
        /// </summary>
        public float radius;

        /// <summary>
        ///     星球使用样式编号
        /// </summary>
        public int meshNum;

        /// <summary>
        ///     是否可以影响其他星球
        /// </summary>
        public bool enableAffect;

        /// <summary>
        ///     是否可以被追踪
        /// </summary>
        public bool enableTracing;

        /// <summary>
        ///     是否为中心天体
        /// </summary>
        public bool isCore;

        public AstralBodyDict()
        {
            
        }
        
        public AstralBodyDict(Transform transform, T astralBody, bool isCore)
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
            this.isCore   = isCore;
            
            period                  = 0;
            radius                  = 0;

        }

    }
}