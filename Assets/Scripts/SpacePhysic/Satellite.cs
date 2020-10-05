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
        private float _c;
        private float _directrix;
        private float _eccentricity;
        private float _centerFactor;
        private float _alpha;
        private Vector3 _xTo3D;
        private Vector3 _yTo3D;

        public Vector3 CalculateOrbit(int t, int totalNumber)
        {
            var angle = t * 2.0f * Mathf.PI / totalNumber;
            var x = _a * Mathf.Cos(angle);
            var y = _b * Mathf.Sin(angle);

            // Debug.Log("A: " + a);
            // Debug.Log("B: " + b);
            // Debug.Log("F: " + f);
            Quaternion q = Quaternion.AngleAxis(_alpha, new Vector3(0, 0, 1));
            Vector3 point = new Vector3(x - _c, y, 0);
            //旋转椭圆轨道
            point = q * point;
            //转换到三维空间
            // point = point.x * _xTo3D + point.y * _yTo3D;
            point = point + _affectedPlanets[0].transform.position;
            return point;
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
            //卫星的位置矢量
            _r = transform.position - _affectedPlanets[0].transform.position;
            //位矢在x正方向的投影
            Vector3 rdx = Vector3.Dot(_r, Vector3.right) * Vector3.right;
            var rr = new Vector3(_r.magnitude, 0, 0);
            
            //椭圆轨道的长短半轴a b
            _a = _r.magnitude * _miu / (2 * _miu - _r.magnitude * Mathf.Pow(velocity.magnitude, 2));
            _b = Vector3.Cross(_r, velocity).magnitude * Mathf.Sqrt(_a / _miu);
            //半焦距
            _c = Mathf.Sqrt(_a * _a - _b * _b);
            //围绕星球在焦点上
            _centerFactor = _a < Mathf.Abs(transform.position.x) ? 1 : -1;
            _centerFactor *= Mathf.Sign(rdx.x);
            
            //准线
            _directrix = _a * _a / _c;
            //离心率
            _eccentricity = _c / _a;
            
            //卫星所在点在平面中的绝对坐标
            float x0 = _directrix - _r.magnitude / _eccentricity;
            float y0 = Mathf.Sqrt(Mathf.Abs(_r.magnitude * _r.magnitude - (x0 - _c) * (x0 - _c)));
            //卫星所在点在平面中与焦点的绝对向量
            Vector3 r0 = new Vector3(x0 - _c, y0, 0);
        
            //v0是在当前r0情况下应该具有的速度，用夹角来确定y0应该为正还是为负
            Vector3 v0 = new Vector3(-y0 * _a / _b, x0 * _a / _b, 0);
            if (Vector3.Angle(velocity, _r) > Vector3.Angle(v0, r0))
                y0 = -y0;
            
            //先求出在正坐标系中与质心距离为r的点的矢量r0，然后测量需要倾斜alpha使行星可以位于轨道上
            r0 = new Vector3(x0 - _c, y0, 0);
            //如果r处在XOY平面中可以直接用r替换rr
            _alpha = Vector3.Angle(r0, _r);
            if (Vector3.Cross(r0, rr).z < 0) 
                _alpha = -_alpha;
            
            // Vector3 normal = Vector3.Cross(velocity, _r);
            // _yTo3D = Vector3.Cross(_r, normal);
            // _yTo3D = _yTo3D / _yTo3D.magnitude;
            // _xTo3D = _r / _r.magnitude;
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