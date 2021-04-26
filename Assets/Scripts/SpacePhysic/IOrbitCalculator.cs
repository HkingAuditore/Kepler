using System.Collections.Generic;
using UnityEngine;

namespace SpacePhysic
{
    /// <summary>
    /// 桂东计算接口
    /// </summary>
    public interface IOrbitCalculator
    {
        List<Vector3> OrbitPoints { get; }
        Vector3       CalculateOrbit(int     t,            int totalNumber);
        void          DrawOrbit(LineRenderer lineRenderer, int totalNumber);
        void          GenerateOrbit();
    }
}