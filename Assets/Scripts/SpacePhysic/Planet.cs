using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpacePhysic
{

    public class Planet : MonoBehaviour, IOrbitCalculator
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

        #region 开普勒定律

        //计算开普勒常数K
        public float GetPlanetK() => PhysicBase.GetG() * this.Mass / (4 * Mathf.PI * Mathf.PI);
        public float GetPlanetMiu() => PhysicBase.GetG() * this.Mass;
        #endregion

        #region 牛顿万有引力

        //计算目标到本星球的引力
        private float CalculateGravityModulus(float targetMass, float distance) =>
            PhysicBase.GetG() * (Mass * targetMass / (distance * distance));

        public Vector3 GetGravityVector3(Rigidbody rigidbody)
        {
            float distance = Vector3.Distance(this.transform.position, rigidbody.position);
            Vector3 normalizedDirection = (this.transform.position - rigidbody.position).normalized;
            return CalculateGravityModulus(rigidbody.mass, distance) * normalizedDirection;
        }
        
        //计算线速度向量
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
        

        #endregion
        
        #region ITrackCalculator

        public List<Vector3> OrbitPoints { get; } = new List<Vector3>();

        
        public Vector3 CalculateOrbit(int t, int totalNumber)
        {
            throw new NotImplementedException();
        }

        public void DrawOrbit(LineRenderer lineRenderer, int totalNumber)
        {
            throw new NotImplementedException();
        }

        public void GenerateOrbit()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}