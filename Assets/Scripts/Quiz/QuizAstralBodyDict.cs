using UnityEngine;

namespace Quiz
{
    /// <summary>
    /// 问题星体传递
    /// </summary>
    public class QuizAstralBodyDict : AstralBodyDict<QuizAstralBody>
    {
        public bool isTarget;
        public QuizAstralBodyDict(Transform transform, QuizAstralBody astralBody, bool isTarget,bool isCore) : base(transform, astralBody, isCore)
        {
            this.isTarget = isTarget;
        }
        
    }
}