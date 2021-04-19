using System;
using UnityEngine;

namespace Quiz
{
    [Serializable]
    public class AstralBodyDict
    {
        public Transform      transform;
        public QuizAstralBody astralBody;

        [SerializeField] private bool _isTarget;

        public AstralBodyDict(Transform transform, QuizAstralBody astralBody, bool isTarget)
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