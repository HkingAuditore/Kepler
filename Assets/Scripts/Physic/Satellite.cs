using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physic
{
   public class Satellite : MonoBehaviour, ITrackCalculator
   {
      private List<Planet> _affectedPlanets = new List<Planet>();
      private LineRenderer _lineRenderer;
      
      public float orbitAltitude;
      public Vector3 velocity;

      private void Start()
      {
         this._lineRenderer = this.GetComponent<LineRenderer>();
      }

      private void Update()
      {
         ShowTrack(_lineRenderer,100);
      }


      public List<Vector3> TrackPoints { get; } = new List<Vector3>();
      

      public Vector3 CalculateTrack(int t, int totalNumber)
      {
         //Vector3 r = this.transform.position - _affectedPlanets[0].transform.position;
         //Vector3 v = velocity - this.transform.position;
         //利用轨道线速度公式求出轨道半长轴
        // float w = 1 / ( 2 / r.magnitude - Mathf.Pow(v.magnitude, 2) / PhysicBase.GetG() * _affectedPlanets[0].Mass);
         float w = 2;
         //利用开普勒第二定律求出轨道半短轴
        // float h = Vector3.Cross(v, r).magnitude * Mathf.Sqrt(w / PhysicBase.GetG() * _affectedPlanets[0].Mass);
         float h = 1;
         //求出焦距
         //float f = Mathf.Sqrt(w * w - h * h);
         float angle = t * 2.0f * Mathf.PI / totalNumber;
         float x = w * Mathf.Cos(angle);
         float y = h * Mathf.Sin(angle);
         
         return new Vector3(x, y, 0) - this.transform.position;
      }

      public void ShowTrack(LineRenderer lineRenderer, int totalNumber)
      {
         TrackPoints.Clear();
         for(int i = 0; i <= totalNumber; i++)
         {
            TrackPoints.Add(CalculateTrack(i,totalNumber));
         }
         _lineRenderer.positionCount = TrackPoints.Count;
         _lineRenderer.SetPositions(TrackPoints.ToArray());
      }

      public void AddAffectedPlanet(Planet planet) => _affectedPlanets.Add(planet);
      public bool IsAffectedPlanetExisted(Planet planet) => _affectedPlanets.Contains(planet);
   }
}
