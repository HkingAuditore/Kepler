using UnityEngine;

namespace Dreamteck
{
    public static class DuplicateUtility
    {
        public static AnimationCurve DuplicateCurve(AnimationCurve input)
        {
            var target = new AnimationCurve();
            target.postWrapMode = input.postWrapMode;
            target.preWrapMode  = input.preWrapMode;
            for (var i = 0; i < input.keys.Length; i++) target.AddKey(input.keys[i]);
            return target;
        }

        public static Gradient DuplicateGradient(Gradient input)
        {
            //yet to implement
            return null;
        }
    }
}