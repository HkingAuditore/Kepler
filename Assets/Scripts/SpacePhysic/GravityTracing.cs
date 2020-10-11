using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpacePhysic;
using UnityEngine;

public class GravityTracing : MonoBehaviour
{
    public int sample = 20;
    public float timeScale = 100;
    private float _deltaTime;
    
    private List<AstralBody> _astralBodies = new List<AstralBody>();
    private Dictionary<AstralBody,LineRenderer> _orbitRenderers = new Dictionary<AstralBody, LineRenderer>();

    //坐标点
    private Dictionary<AstralBody, List<Vector3>> _orbitPoints = new Dictionary<AstralBody, List<Vector3>>();


    private void Start()
    {
        _deltaTime = (100f / sample) * Time.fixedDeltaTime * timeScale;
        foreach (AstralBody astralBody in this.GetComponentsInChildren<AstralBody>())
        {
            _astralBodies.Add(astralBody);
            _orbitPoints[astralBody] = new List<Vector3> {astralBody.transform.position};
            _orbitRenderers[astralBody] = astralBody.gameObject.GetComponent<LineRenderer>();
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

    private Vector3 GetGravityVector3(AstralBody a0,AstralBody a1,int sampleTime)
    {
        float distance = Vector3.Distance(_orbitPoints[a0][sampleTime], _orbitPoints[a1][sampleTime]);
        Vector3 normalizedDirection = (_orbitPoints[a1][sampleTime] - _orbitPoints[a0][sampleTime]).normalized;
        return CalculateGravityModulus(a0.Mass,a1.Mass, distance) * normalizedDirection;
    }
    
    private Vector3 CalculateForce(AstralBody astralBody,int sampleTime)
    {
        Vector3 result = new Vector3(0,0,0);
        //求合力
        foreach (AstralBody body in astralBody.AffectedPlanets)
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
        Dictionary<AstralBody, Vector3> astralBodyVelocities = new Dictionary<AstralBody, Vector3>();


        foreach (AstralBody astralBody in _astralBodies)
        {
            //起始速度
            astralBodyVelocities[astralBody] = astralBody.GetComponent<Rigidbody>().velocity;
            //起始点改为当前位置
            if(_orbitPoints.ContainsKey(astralBody))_orbitPoints[astralBody].Clear();
            _orbitPoints[astralBody].Add(astralBody.transform.position);
        }

        //开始采样
        for (int i = 0; i < sample; i++)
        {
            //遍历星体
            foreach (AstralBody astralBody in _astralBodies)
            {
                //加速度
                /*
                 * F=ma
                 * delta v=at
                 * s = vt + 0.5a(t^2)
                 */
                Vector3 acceleration = CalculateForce(astralBody,i) / astralBody.Mass;
                _orbitPoints[astralBody].Add(_orbitPoints[astralBody].Last()+
                                             astralBodyVelocities[astralBody] * _deltaTime +
                                             .5f * acceleration * Mathf.Pow(_deltaTime , 2));
                //加速后速度
                astralBodyVelocities[astralBody] += acceleration * _deltaTime;
                //Debug.DrawLine(_orbitPoints[astralBody].Last(),_orbitPoints[astralBody].Last() + CalculateForce(astralBody,i));
                

            }
        }
    }

    public void DrawOrbit(AstralBody astralBody)
    {
        _orbitRenderers[astralBody].positionCount = sample;
        _orbitRenderers[astralBody].SetPositions(_orbitPoints[astralBody].ToArray());
    }

    private void DrawOrbits()
    {
        TraceGravity();
        foreach (AstralBody astralBody in _astralBodies)
        {
            DrawOrbit(astralBody);
        }
    }

    #endregion
}