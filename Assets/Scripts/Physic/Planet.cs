using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Physic
{
    public class PhysicBase
    {
        public static float G = 6.67f;
        public static float GetG() => PhysicBase.G * Mathf.Pow(10, -11);
    }
    public class Planet : MonoBehaviour, ITrackCalculator
    {
        
        public float radius;

        public float Mass { get; private set; }
        private Rigidbody _rigidbody;
        private List<Planet> _affectedPlanets = new List<Planet>();
        private LineRenderer _lineRenderer;


        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _rigidbody = GetComponent<Rigidbody>();
            Mass = _rigidbody.mass;

        }
        
        private void OnTriggerStay(Collider other)
        {
            Satellite satellite = other.GetComponent<Satellite>();
            if (satellite != null)
            {
                if(!satellite.IsAffectedPlanetExisted(this))
                    satellite.AddAffectedPlanet(this);
            }
        }


        //计算目标到本星球的引力
        private float CalculateGravityModulus(float targetMass, float distance) =>
            PhysicBase.GetG() * (Mass * targetMass / (distance * distance));

        public Vector3 GetGravityVector3(Rigidbody rigidbody)
        {
            float distance = Vector3.Distance(this.transform.position, rigidbody.position);
            Vector3 normalizedDirection = (this.transform.position - rigidbody.position).normalized;
            return CalculateGravityModulus(rigidbody.mass, distance) * normalizedDirection;
        }

        public Vector3 CalculateLinearVelocity()
        {
            List<Vector3> gravities = new List<Vector3>();
            
            //计算引力向量集
            foreach (Planet planet in _affectedPlanets)
            {
                float distance = Vector3.Distance(this.transform.position, planet.gameObject.transform.position);
                gravities.Add(planet.GetGravityVector3(this._rigidbody));
            }

            //计算合力
            Vector3 forceResult = gravities.Aggregate((r, v) => r + v);
            //TODO 计算速度
            Vector3 velocity = forceResult;

            return velocity;
        }

        public List<Vector3> TrackPoints { get; } = new List<Vector3>();

        
        public Vector3 CalculateTrack(int t, int totalNumber)
        {
            throw new System.NotImplementedException();
        }

        public void ShowTrack(LineRenderer lineRenderer, int totalNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}