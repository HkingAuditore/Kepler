using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpacePhysic
{
    public interface ITraceable
    {
        Transform GetTransform();
        GameObject GetGameObject();
        bool GetEnableTracing();
        float GetMass();
        Rigidbody GetRigidbody();
        List<AstralBody> GetAffectedPlanets();
    }
    public class GravityTracing : MonoBehaviour
    {
        public int sample = 20;
        public float timeScale = 100;
        private float _deltaTime;
    
        private List<ITraceable> _astralBodies = new List<ITraceable>();
        private Dictionary<ITraceable,LineRenderer> _orbitRenderers = new Dictionary<ITraceable, LineRenderer>();

        //坐标点
        private Dictionary<ITraceable, List<Vector3>> _orbitPoints = new Dictionary<ITraceable, List<Vector3>>();


        private void Start()
        {
            _deltaTime = (100f / sample) * Time.fixedDeltaTime * timeScale;
            foreach (ITraceable traceable in this.GetComponentsInChildren<AstralBody>())
            {
                AddTracingTarget(traceable);
            }
        }

        public void AddTracingTarget(ITraceable traceable)
        {
            if (traceable.GetEnableTracing())
            {
                _astralBodies.Add(traceable);
                _orbitPoints[traceable] = new List<Vector3> {traceable.GetTransform().position};
                _orbitRenderers[traceable] = traceable.GetTransform().Find("Line").gameObject.GetComponent<LineRenderer>();
            }
        }

        private void FixedUpdate()
        {
            DrawOrbits();
        }
    
        #region 引力步进

        #region 引力计算

        //计算两个星体之间的引力
        private float CalculateGravityModulus(float originMass, float targetMass, float distance) =>
            PhysicBase.GetG() * (originMass * targetMass / (distance * distance));

        private Vector3 GetGravityVector3(ITraceable a0,ITraceable a1,int sampleTime)
        {
            float distance = Vector3.Distance(_orbitPoints[a0][sampleTime], _orbitPoints[a1][sampleTime]);
            Vector3 normalizedDirection = (_orbitPoints[a1][sampleTime] - _orbitPoints[a0][sampleTime]).normalized;
            return CalculateGravityModulus(a0.GetMass(),a1.GetMass(), distance) * normalizedDirection;
        }
    
        private Vector3 CalculateForce(ITraceable astralBody,int sampleTime)
        {
            Vector3 result = new Vector3(0,0,0);
            //求合力
            foreach (ITraceable body in astralBody.GetAffectedPlanets())
            {
                if(body == astralBody) continue;
                result += GetGravityVector3(astralBody,body,sampleTime);
            }

            //Debug.Log(astralBody.name + " force: " + result);
            return result;
        }

        #endregion
    
        //引力步进
        private void TraceGravity()
        {
            Dictionary<ITraceable, Vector3> astralBodyVelocities = new Dictionary<ITraceable, Vector3>();


            foreach (ITraceable astralBody in _astralBodies)
            {
                //起始速度
                astralBodyVelocities[astralBody] = astralBody.GetRigidbody().velocity;
                //起始点改为当前位置
                if(_orbitPoints.ContainsKey(astralBody))_orbitPoints[astralBody].Clear();
                _orbitPoints[astralBody].Add(astralBody.GetTransform().position);
            }

            //开始采样
            for (int i = 0; i < sample; i++)
            {
                //遍历星体
                foreach (ITraceable astralBody in _astralBodies)
                {
                    //加速度
                    /*
                 * F=ma
                 * delta v=at
                 * s = vt + 0.5a(t^2)
                 */
                    Vector3 acceleration = CalculateForce(astralBody,i) / astralBody.GetMass();
                    _orbitPoints[astralBody].Add(_orbitPoints[astralBody].Last()+
                                                 astralBodyVelocities[astralBody] * _deltaTime +
                                                 .5f * acceleration * Mathf.Pow(_deltaTime , 2));
                    //加速后速度
                    astralBodyVelocities[astralBody] += acceleration * _deltaTime;
                    //Debug.DrawLine(_orbitPoints[astralBody].Last(),_orbitPoints[astralBody].Last() + CalculateForce(astralBody,i));
                

                }
            }
        }

        public void DrawOrbit(ITraceable astralBody)
        {
            _orbitRenderers[astralBody].positionCount = sample;
            _orbitRenderers[astralBody].SetPositions(_orbitPoints[astralBody].ToArray());
        }

        private void DrawOrbits()
        {
            TraceGravity();
            foreach (ITraceable astralBody in _astralBodies)
            {
                DrawOrbit(astralBody);
            }
        }

        #endregion
    }
}