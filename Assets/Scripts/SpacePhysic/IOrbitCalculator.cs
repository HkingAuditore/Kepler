using System.Collections.Generic;
using UnityEngine;

namespace SpacePhysic
{
    public static class PhysicBase
    {
        public static float G = 6.67f;
        public static float GetG() => PhysicBase.G * Mathf.Pow(10, 0);
    }
    public interface IOrbitCalculator
    {
        List<Vector3> OrbitPoints { get; }
        Vector3 CalculateOrbit(int t,int totalNumber);
        void DrawOrbit(LineRenderer lineRenderer, int totalNumber);
    }
}
