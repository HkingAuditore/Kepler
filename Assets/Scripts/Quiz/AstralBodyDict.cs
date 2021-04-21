using System;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Quiz
{
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