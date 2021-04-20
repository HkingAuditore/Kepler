using UnityEngine;

namespace SpacePhysic
{
    public static class PhysicBase
    {
        /// <summary>
        ///     万有引力常量（小数部分）
        /// </summary>
        public static float G = 6.67f;

        public static float GetG()
        {
            return G * Mathf.Pow(10, 0);
        }

        public static double GetRealG()
        {
            return G * Mathf.Pow(10, -11);
        }
    }
}