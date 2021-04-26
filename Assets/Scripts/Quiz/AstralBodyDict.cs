using System;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Quiz
{
    /// <summary>
    /// 星体数据传递
    /// </summary>
    /// <typeparam name="T">存储类型</typeparam>
    [Serializable]
    public class AstralBodyDict<T> where T : AstralBody
    {
        public bool      isCore;
        public                                                    T         astralBody;
        public                                                    Transform transform;

        public AstralBodyDict(Transform transform, T astralBody, bool isCore)
        {
            this.transform  = transform;
            this.astralBody = astralBody;
            this.isCore     = isCore;
        }
        
    }
}