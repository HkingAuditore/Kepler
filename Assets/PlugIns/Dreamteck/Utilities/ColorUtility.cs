using UnityEngine;

namespace Dreamteck
{
    public static class ColorUtility
    {
        public static Color MoveTowardsColor(Color from, Color to, float t)
        {
            var clrf   = new Vector4(from.r, from.g, from.b, from.a);
            var clrt   = new Vector4(to.r,   to.g,   to.b,   to.a);
            var result = Vector4.MoveTowards(clrf, clrt, t);
            return new Color(result.x, result.y, result.z, result.w);
        }
    }
}