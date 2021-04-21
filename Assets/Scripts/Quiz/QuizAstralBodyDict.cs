using UnityEngine;

namespace Quiz
{
    public class QuizAstralBodyDict : AstralBodyDict<QuizAstralBody>
    {
        public bool isTarget;
        public QuizAstralBodyDict(Transform transform, QuizAstralBody astralBody, bool isTarget) : base(transform, astralBody, isTarget)
        {
            this.isTarget = isTarget;
        }
    }
}