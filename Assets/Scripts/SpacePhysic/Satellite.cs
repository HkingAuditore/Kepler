using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpacePhysic
{
   public class Satellite : MonoBehaviour, IOrbitCalculator
   {
      private List<Planet> _affectedPlanets = new List<Planet>();
      private LineRenderer _lineRenderer;
      
      public float orbitAltitude;
      public Vector3 velocity;
      private Rigidbody _rigidbody;
      public float Mass { get; private set; }

      private void Start()
      {
         this._lineRenderer = this.GetComponent<LineRenderer>();
         _lineRenderer = GetComponent<LineRenderer>();
         _rigidbody = GetComponent<Rigidbody>();
         Mass = _rigidbody.mass;
      }

      private void Update()
      {
         DrawOrbit(_lineRenderer,20);
      }


      public List<Vector3> OrbitPoints { get; } = new List<Vector3>();

      public Vector3 CalculateOrbit(int t, int totalNumber)
      {
         //环绕星球的标准重力参数
         float miu = _affectedPlanets[0].GetPlanetMiu();
         // 卫星到中心天体的距离
         Vector3 r = this.transform.position - _affectedPlanets[0].transform.position;
         //椭圆轨道的长短半轴a b
         float a = r.magnitude*miu/(2*miu-r.magnitude*Mathf.Pow(velocity.magnitude,2));
         float b = Vector3.Cross(r, velocity).magnitude * Mathf.Sqrt(a / miu);
         //椭圆轨道的焦距
         float f = Mathf.Sqrt(a * a - b * b);
         float angle = t * 2.0f * Mathf.PI / totalNumber;
         float x = a * Mathf.Cos(angle);
         float y = b * Mathf.Sin(angle);
         //开普勒第一定律，围绕星球在焦距上
         //通过位矢与(1,0,0）的叉积判断方向
         int centerFactor = a < this.transform.position.x ? 1 : -1;
         Debug.Log("A: " + a);
         Debug.Log("B: " + b);
         Debug.Log("F: " + f);
         return new Vector3(x+f*centerFactor, y, 0) + _affectedPlanets[0].transform.position;
      }

      public void DrawOrbit(LineRenderer lineRenderer, int totalNumber)
      {
         OrbitPoints.Clear();
         for(int i = 0; i <= totalNumber; i++)
         {
            OrbitPoints.Add(CalculateOrbit(i,totalNumber));
         }
         _lineRenderer.positionCount = OrbitPoints.Count;
         _lineRenderer.SetPositions(OrbitPoints.ToArray());
      }

      public void AddAffectedPlanet(Planet planet) => _affectedPlanets.Add(planet);
      public bool IsAffectedPlanetExisted(Planet planet) => _affectedPlanets.Contains(planet);
   }
}
