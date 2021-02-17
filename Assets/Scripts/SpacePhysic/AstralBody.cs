using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpacePhysic;
using UnityEngine;

public class AstralBody : MonoBehaviour,ITraceable
{
    [Header("Basic Property")]
    public float mass;
    public float Mass
    {
        get => mass;
        set
        {
            mass = value;
            SetMass();
        }
    }

    public float density;
    public float originalSize = 1;
    public float curSize;
    public Rigidbody AstralBodyRigidbody { get; set; }
    public SphereCollider triggerCollider;
    public SphereCollider defaultCollider;
    [Header("Movement Property")]
    public Vector3 oriVelocity;
    public Vector3 angularVelocity;
    [Header("Gravity Property")]
    public bool enableAffect = true;
    public float affectRadius;
    public bool enableTracing = false;

    public Vector3 Force { get; private set; }


    public List<AstralBody> AffectedPlanets { get; } = new List<AstralBody>();



    protected virtual void Start()
    {
        AstralBodyRigidbody = GetComponent<Rigidbody>();
        SetMass();
        ChangeSize(originalSize);
        

        
        
        this.AstralBodyRigidbody.angularVelocity = this.angularVelocity;

        ChangeVelocity(oriVelocity);
    }

    private void FixedUpdate()
    {
        var force = CalculateForce();
        //Debug.Log(this.name + " force: " + force);
        // Debug.DrawLine(transform.position,transform.position + force,Color.green);
        this.AstralBodyRigidbody.AddForce(force);
    }


    private void OnTriggerEnter(Collider other)
    {
       var astral =  other.GetComponent<AstralBody>();
       if (astral != null && !other.isTrigger && this.enableAffect && !astral.AffectedPlanets.Contains(astral))
       {
           astral.AffectedPlanets.Add(this);
           // Debug.Log(this.gameObject.name + " In Eff :" + astral.gameObject.name);
       }
    }

    private void OnTriggerExit(Collider other)
    {
        var astral =  other.GetComponent<AstralBody>();
        if (astral != null && this.enableAffect && astral.AffectedPlanets.Contains(astral))
        {
            astral.AffectedPlanets.Remove(this);
        }
    }
    
  
    
    #region 开放修改参数

    //调整星球体积
    public void ChangeSize(float size)
    {
        this.transform.localScale = new Vector3(size, size, size);
        // defaultCollider.radius *= size;
        // if(enableAffect)
        //     triggerCollider.radius = affectRadius * size;
        curSize = size;
    }
    //调整星球密度

    public void ChangeVelocity(Vector3 velocity)
    {
        AstralBodyRigidbody.velocity = velocity;
    }

    public virtual void SetMass()
    {
        // AstralBodyRigidbody.mass = Mathf.PI * (4/3)*Mathf.Pow(curSize,3) * this.density;
        AstralBodyRigidbody.mass = this.Mass;
    }

    public void ChangeMass(float curMass)
    {
        this.Mass = curMass;
        SetMass();
    }
    public void ChangeDensity(float curDensity)
    {
        this.density = curDensity;
    }
    #endregion 开放修改参数


    #region 牛顿万有引力

    //计算目标到本星球的引力
    private float CalculateGravityModulus(float targetMass, float distance) =>
        PhysicBase.GetG() * (this.GetMass() * targetMass / (distance * distance));

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
        foreach (AstralBody astralBody in AffectedPlanets)
        {
            //float distance = Vector3.Distance(this.transform.position, astralBody.gameObject.transform.position);
            forceResult+=astralBody.GetGravityVector3(this.AstralBodyRigidbody);
            //gravities.Add(astralBody.GetGravityVector3(this._rigidbody));
        }

        //计算合力
        //Vector3 forceResult = gravities.Aggregate((r, v) => r + v);
        Force = forceResult;
        return forceResult;
    }


    #endregion

    #region 引力步进计算轨道

    

    #endregion

    public Transform GetTransform() => this.transform;


    public GameObject GetGameObject() => this.gameObject;


    public bool GetEnableTracing() => this.enableTracing;


    public virtual float GetMass() => this.AstralBodyRigidbody.mass;


    public Rigidbody GetRigidbody() => this.AstralBodyRigidbody;


    public List<AstralBody> GetAffectedPlanets() => this.AffectedPlanets;

}
