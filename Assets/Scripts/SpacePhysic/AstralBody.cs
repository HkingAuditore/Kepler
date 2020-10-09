using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpacePhysic;
using UnityEngine;

public class AstralBody : MonoBehaviour
{
    public float Mass { get; private set; }
    public Vector3 oriVelocity;
    public float radius;
    private Rigidbody _rigidbody;
    public List<AstralBody> affectedPlanets = new List<AstralBody>();
    private LineRenderer _lineRenderer;    
    
    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
        this.GetComponents<SphereCollider>()[1].radius = radius;
        Mass = _rigidbody.mass;

        _rigidbody.velocity = oriVelocity;

    }

    private void FixedUpdate()
    {
        var force = CalculateForce();
        //Debug.Log(this.name + " force: " + force);

        this._rigidbody.AddForce(force);
    }


    private void OnTriggerEnter(Collider other)
    {
       var astral =  other.GetComponent<AstralBody>();
       if (astral != null)
       {
           astral.affectedPlanets.Add(this);
       }
    }

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
    public Vector3 CalculateForce()
    {
        //List<Vector3> gravities = new List<Vector3>();
        Vector3 forceResult = new Vector3(0,0,0);
            
        //计算引力向量集
        foreach (AstralBody astralBody in affectedPlanets)
        {
            //float distance = Vector3.Distance(this.transform.position, astralBody.gameObject.transform.position);
            forceResult+=astralBody.GetGravityVector3(this._rigidbody);
            //gravities.Add(astralBody.GetGravityVector3(this._rigidbody));
        }

        //计算合力
        //Vector3 forceResult = gravities.Aggregate((r, v) => r + v);

        return forceResult;
    }
        

    #endregion

    #region 引力步进计算轨道

    

    #endregion
}
