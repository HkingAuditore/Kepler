using System.Collections.Generic;
using UnityEngine;

namespace SpacePhysic
{
    public class Satellite : MonoBehaviour, IOrbitCalculator
    {
        private readonly List<Planet> _affectedPlanets = new List<Planet>();
        private LineRenderer _lineRenderer;

        public float orbitAltitude;
        public Vector3 velocity;
        private Rigidbody _rigidbody;
        public float Mass { get; private set; }

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer = GetComponent<LineRenderer>();
            _rigidbody = GetComponent<Rigidbody>();
            Mass = _rigidbody.mass;
        }

        private void Update()
        {
            DrawOrbit(_lineRenderer, 20);
        }


        public List<Vector3> OrbitPoints { get; } = new List<Vector3>();
        private float _miu;
        private Vector3 _r;
        private float _a;
        private float _b;
        private float _f;

        public Vector3 CalculateOrbit(int t, int totalNumber)
        {
            var angle = t * 2.0f * Mathf.PI / totalNumber;
            var x = _a * Mathf.Cos(angle);
            var y = _b * Mathf.Sin(angle);

            //围绕星球在焦距上
            var centerFactor = _a < transform.position.x ? 1 : -1;
            // Debug.Log("A: " + a);
            // Debug.Log("B: " + b);
            // Debug.Log("F: " + f);
            return new Vector3(x + _f * centerFactor, y, 0) + _affectedPlanets[0].transform.position;
        }

        public void DrawOrbit(LineRenderer lineRenderer, int totalNumber)
        {
            GenerateOrbit();
            OrbitPoints.Clear();
            for (var i = 0; i <= totalNumber; i++) OrbitPoints.Add(CalculateOrbit(i, totalNumber));
            _lineRenderer.positionCount = OrbitPoints.Count;
            _lineRenderer.SetPositions(OrbitPoints.ToArray());
        }

        public void GenerateOrbit()
        {
            //环绕星球的标准重力参数
            _miu = _affectedPlanets[0].GetPlanetMiu();

            _r = transform.position - _affectedPlanets[0].transform.position;
            //椭圆轨道的长短半轴a b
            _a = _r.magnitude * _miu / (2 * _miu - _r.magnitude * Mathf.Pow(velocity.magnitude, 2));
            _b = Vector3.Cross(_r, velocity).magnitude * Mathf.Sqrt(_a / _miu);

            _f = Mathf.Sqrt(_a * _a - _b * _b);
        }

        public void AddAffectedPlanet(Planet planet)
        {
            _affectedPlanets.Add(planet);
        }

        public bool IsAffectedPlanetExisted(Planet planet)
        {
            return _affectedPlanets.Contains(planet);
        }
    }
}