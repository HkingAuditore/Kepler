using System;
using SpacePhysic;
using UnityEngine;

namespace Quiz
{
    [Serializable]
    public class AstralBodyDict<T> where T : AstralBody
    {
        [SerializeField] private bool           _isTarget;
        public                   T astralBody;
        public                   Transform      transform;

        public AstralBodyDict(Transform transform, T astralBody, bool isTarget)
        {
            this.transform  = transform;
            this.astralBody = astralBody;
            this.isTarget   = isTarget;
        }

        public bool isTarget
        {
            get =>
                // Debug.Log(astralBody.gameObject.name + " is target? : " + _isTarget);
                _isTarget;
            set => _isTarget = value;
            // Debug.Log(astralBody.gameObject.name + " set to " + _isTarget);
        }
    }
}