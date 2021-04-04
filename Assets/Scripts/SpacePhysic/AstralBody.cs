using System;
using System.Collections.Generic;
using System.Linq;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class AstralBody : MonoBehaviour, ITraceable
{
    [Header("Basic Property")] public float mass;

    public                                        float          density;
    [FormerlySerializedAs("originalSize")] public float          _size = 1;
    private                                       int            _meshNum;
    public                                        SphereCollider triggerCollider;
    public                                        SphereCollider defaultCollider;

    [Header("Movement Property")] public Vector3 oriVelocity;

    public Vector3 angularVelocity;
    

    [Header("Gravity Property")] public bool enableAffect = true;

    public float affectRadius;
    public bool  enableTracing;

    public List<AstralBody> affectedPlanets = new List<AstralBody>();
    public List<AstralBody> banAffectedPlanets = new List<AstralBody>();

    private Vector3 _lastVelocity;

    public float Mass
    {
        get => mass;
        set
        {
            mass = value;
            SetMass();
        }
    }

    public Rigidbody AstralBodyRigidbody { get; set; }

    public Vector3 Force { get; private set; }
    
    public float gravity
    {
        get => (SpacePhysic.PhysicBase.GetG() * this.Mass) / (this.size * this.size);
    }

    private MeshFilter            _mesh;
    private Renderer              _renderer;
    public  UnityEvent<Vector3> velocityChangedEvent;

    public int meshNum
    {
        get => _meshNum;
        set
        {
            _meshNum  = value;
            List<Material> materials = new List<Material>();
            this._mesh.sharedMesh = GameManager.GetGameManager.GetMeshAndMaterialsFromList(_meshNum,ref materials);
            this._renderer.sharedMaterials = materials.ToArray();
        }
    }

    public float size
    {
        get => _size;
        set
        {
            _size = value;
            ChangeSize();
        }
    }

    private void Awake()
    {
        _mesh               = this.GetComponent<MeshFilter>();
        _renderer           = this.GetComponent<Renderer>();
        AstralBodyRigidbody = GetComponent<Rigidbody>();

    }

    protected virtual void Start()
    {
        
        triggerCollider.radius =  affectRadius;
        defaultCollider.radius *= 1.2f;
        SetMass();
        ChangeSize();

        AstralBodyRigidbody.angularVelocity = angularVelocity;
        _lastVelocity                       = oriVelocity;
        ChangeVelocity(oriVelocity);
    }

    protected virtual void FixedUpdate()
    {
        var force                                           = CalculateForce();
        if (!AstralBodyRigidbody.isKinematic) _lastVelocity = AstralBodyRigidbody.velocity;
        //Debug.Log(this.name + " force: " + force);
        // Debug.DrawLine(transform.position,transform.position + force,Color.green);
        AstralBodyRigidbody.AddForce(force);
    }


    private void OnTriggerEnter(Collider other)
    {
        var astral = other.GetComponent<AstralBody>();
        if (astral != null && !other.isTrigger && enableAffect && !astral.banAffectedPlanets.Contains(this) && !astral.affectedPlanets.Contains(this))
            astral.affectedPlanets.Add(this);
    }

    private void OnTriggerExit(Collider other)
    {
        var astral = other.GetComponent<AstralBody>();
        if (astral != null && enableAffect && astral.affectedPlanets.Contains(astral))
            astral.affectedPlanets.Remove(this);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }


    public GameObject GetGameObject()
    {
        return gameObject;
    }


    public bool GetEnableTracing()
    {
        return enableTracing;
    }


    public virtual float GetMass()
    {
        return GetRigidbody().mass;
    }


    public Rigidbody GetRigidbody()
    {
        if (AstralBodyRigidbody == null) AstralBodyRigidbody = GetComponent<Rigidbody>();
        return AstralBodyRigidbody;
    }

    public Vector3 GetVelocity()
    {
        return _lastVelocity;
    }


    public List<AstralBody> GetAffectedPlanets()
    {
        return affectedPlanets;
    }

    public AstralBody GetAstralBody()
    {
        return this;
    }
    



    #region 开放修改参数

    //调整星球体积
    private void ChangeSize()
    {
        transform.localScale = new Vector3(size, size, size);
        // defaultCollider.radius *= size;
        // if(enableAffect)
        //     triggerCollider.radius = affectRadius * size;
        // size = curSize;
    }
    //调整星球密度

    public void ChangeVelocity(Vector3 velocity)
    {
        if (!AstralBodyRigidbody.isKinematic)
        {
            AstralBodyRigidbody.velocity = velocity;
        }
        else
        {
            this._lastVelocity                   = velocity;
            this.AstralBodyRigidbody.isKinematic = false;
            AstralBodyRigidbody.velocity         = velocity;
            this.AstralBodyRigidbody.isKinematic = true;
        }

        velocityChangedEvent.Invoke(velocity);
    }
    
    public void ChangeVelocity(float speed)
    {
        if (!AstralBodyRigidbody.isKinematic)
        {
            AstralBodyRigidbody.velocity = AstralBodyRigidbody.velocity.normalized * speed;
        }
        else
        {
            this._lastVelocity                   = this._lastVelocity.normalized * speed;
            this.AstralBodyRigidbody.isKinematic = false;
            AstralBodyRigidbody.velocity         = this._lastVelocity;
            this.AstralBodyRigidbody.isKinematic = true;
        }
    }

    private protected virtual void SetMass()
    {
        // AstralBodyRigidbody.mass = Mathf.PI * (4/3)*Mathf.Pow(curSize,3) * this.density;
        AstralBodyRigidbody.mass = Mass;
    }

    public void ChangeMass(float curMass)
    {
        Mass = curMass;
        SetMass();
    }

    public void ChangeDensity(float curDensity)
    {
        density = curDensity;
    }

    #endregion 开放修改参数


    #region 牛顿万有引力

    //计算目标到本星球的引力
    private float CalculateGravityModulus(float targetMass, float distance)
    {
        return PhysicBase.GetG() * (GetMass() * targetMass / (distance * distance));
    }

    public Vector3 GetGravityVector3(Rigidbody rigidbody)
    {
        var distance            = Vector3.Distance(transform.position, rigidbody.position);
        var normalizedDirection = (transform.position - rigidbody.position).normalized;
        return CalculateGravityModulus(rigidbody.mass, distance) * normalizedDirection;
    }

    //计算线速度向量
    public Vector3 CalculateForce()
    {
        //List<Vector3> gravities = new List<Vector3>();
        var forceResult = new Vector3(0, 0, 0);

        //计算引力向量集
        foreach (var astralBody in affectedPlanets)
            //float distance = Vector3.Distance(this.transform.position, astralBody.gameObject.transform.position);
            forceResult += astralBody.GetGravityVector3(AstralBodyRigidbody);
        //gravities.Add(astralBody.GetGravityVector3(this._rigidbody));

        //计算合力
        //Vector3 forceResult = gravities.Aggregate((r, v) => r + v);
        Force = forceResult;
        return forceResult;
    }

    #endregion

    #region 引力步进计算轨道

    #endregion             
    
    #region 优化属性设置
    public void SetCircleVelocity()
    {
        //查找引力核心 
        AstralBody core = affectedPlanets.OrderByDescending(a => this.GetGravityVector3(a.GetRigidbody()).magnitude).FirstOrDefault();
        this.ChangeVelocity(MathPlus.CustomSolver.GetCircleOrbitVelocity(this.transform.position,core.GetTransform().position,core.mass));
    }
[ContextMenu("To Circle Velocity")]
    public void SetCircleVelocityMenu()
    {
        //查找引力核心
        AstralBody core = affectedPlanets.OrderByDescending(a => this.GetGravityVector3(a.GetRigidbody()).magnitude).FirstOrDefault();
        this.oriVelocity = (MathPlus.CustomSolver.GetCircleOrbitVelocity(this.transform.position,core.GetTransform().position,core.mass));
    }
    #endregion
}