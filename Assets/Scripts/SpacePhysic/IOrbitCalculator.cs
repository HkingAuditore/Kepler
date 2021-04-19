using System.Collections.Generic;
using UnityEngine;

namespace SpacePhysic
{
    public interface IOrbitCalculator
    {
        List<Vector3> OrbitPoints { get; }
        Vector3       CalculateOrbit(int     t,            int totalNumber);
        void          DrawOrbit(LineRenderer lineRenderer, int totalNumber);
        void          GenerateOrbit();
    }
}