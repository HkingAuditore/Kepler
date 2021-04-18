using System;
using System.Collections.Generic;
using System.Linq;
using GameManagers;
using MathPlus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SpacePhysic
{
    public class AstralBody : MonoBehaviour, ITraceable
    {
        [FormerlySerializedAs("mass")] [Header("Basic Property")] [SerializeField]
        private float _mass;

        [FormerlySerializedAs("originalSize")] [SerializeField]
        private float _size = 1;

        public SphereCollider triggerCollider;
        public SphereCollider defaultCollider;

        [Header("Movement Property")] public Vector3 oriVelocity;

        public Vector3 angularVelocity;


        [Header("Gravity Property")] public bool enableAffect = true;

        public float affectRadius;
        public bool  enableTracing;

        public List<AstralBody>    affectedPlanets    = new List<AstralBody>();
        public List<AstralBody>    banAffectedPlanets = new List<AstralBody>();
        public UnityEvent<Vector3> velocityChangedEvent;

        [SerializeField] private double _realMass;

        [SerializeField] private double    _density;
        private                  Rigidbody _astralBodyRigidbody;

        private Vector3 _lastVelocity;

        private MeshFilter _mesh;
        private int        _meshNum;
        private Renderer   _renderer;

        public float Mass
        {
            get => _mass;
            private set
            {
                _mass = value > 0 ? value : 0;
                try
                {
                    SetMass();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public Rigidbody astralBodyRigidbody
        {
            get => _astralBodyRigidbody;
            set
            {
                if (_astralBodyRigidbody == null) _astralBodyRigidbody = GetComponent<Rigidbody>();
                _astralBodyRigidbody = value;
            }
        }

        public Vector3 Force { get; private set; }

        public float gravity
        {
            get
            {
                double realSize = size * Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.M));
                // Debug.Log("RealSize = " + realSize);
                return (float) (PhysicBase.GetRealG() * realMass / (realSize * realSize));
            }
        }

        public int meshNum
        {
            get => _meshNum;
            set
            {
                _meshNum = value;
                var materials = new List<Material>();
                _mesh.sharedMesh          = GameManager.GetGameManager.GetMeshAndMaterialsFromList(_meshNum, ref materials);
                _renderer.sharedMaterials = materials.ToArray();
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

        public double realMass
        {
            get => _realMass;
            set
            {
                _realMass = value;
                // Debug.Log(GameManager.GetGameManager.GetK(PropertyUnit.Kg));
                // Debug.Log( Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.Kg)));
                // Debug.Log("num:" + _realMass       * Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.Kg)));
                Mass = (float) (_realMass * Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.Kg, _realMass)));
                // Debug.Log((_realMass * Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.Kg, _realMass))));
            }
        }

        public double density
        {
            get
            {
                double realSize = size           * Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.M));
                _density = (float) (3 * realMass / (4 * Mathf.PI * realSize * realSize * realSize));
                return _density;
            }
        }


        private void Awake()
        {
            _mesh               = GetComponent<MeshFilter>();
            _renderer           = GetComponent<Renderer>();
            astralBodyRigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            triggerCollider.radius =  affectRadius;
            defaultCollider.radius *= 1.2f;
            SetMass();
            ChangeSize();

            astralBodyRigidbody.angularVelocity = angularVelocity;
            _lastVelocity                       = oriVelocity;
            ChangeVelocity(oriVelocity);
        }

        protected virtual void FixedUpdate()
        {
            var force                                           = CalculateForce();
            if (!astralBodyRigidbody.isKinematic) _lastVelocity = astralBodyRigidbody.velocity;
            //Debug.Log(this.name + " force: " + force);
            // Debug.DrawLine(transform.position,transform.position + force,Color.green);
            astralBodyRigidbody.AddForce(force);
        }

        private void OnDestroy()
        {
            GameManager.GetGameManager.orbit.RemoveAstralBody(this);
        }

        public virtual void OnCollisionEnter(Collision other)
        {
            var effect          = Resources.Load("Effect/Explosion");
            var otherAstralBody = other.gameObject.GetComponent<AstralBody>();
            var destroyBody     = otherAstralBody.Mass > Mass ? this : otherAstralBody;
            var effectGameObj =
                Instantiate(effect, destroyBody.transform.position, destroyBody.transform.rotation);
            Destroy(destroyBody.gameObject);
            Destroy(effectGameObj, 3f);
        }


        private void OnTriggerEnter(Collider other)
        {
            var astral = other.GetComponent<AstralBody>();
            if (astral != null && !other.isTrigger && enableAffect && !astral.banAffectedPlanets.Contains(this) &&
                !astral.affectedPlanets.Contains(this))
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
            return transform.position;
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
            return GetAstralBody().Mass;
        }


        public Rigidbody GetRigidbody()
        {
            if (astralBodyRigidbody == null) astralBodyRigidbody = GetComponent<Rigidbody>();
            return astralBodyRigidbody;
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
            var localScale = GameManager.GetGameManager.globalDistanceScaler <= 0
                ? 1
                : GameManager.GetGameManager.globalDistanceScaler;
            var showSize = Mathf.Pow(size, .2f) * localScale * localScale;
            transform.localScale = new Vector3(showSize, showSize, showSize);
            // defaultCollider.radius *= size;
            // if(enableAffect)
            //     triggerCollider.radius = affectRadius * size;
            // size = curSize;
        }
        //调整星球密度

        public void ChangeVelocity(Vector3 velocity)
        {
            if (!astralBodyRigidbody.isKinematic)
            {
                astralBodyRigidbody.velocity = velocity;
            }
            else
            {
                _lastVelocity                   = velocity;
                astralBodyRigidbody.isKinematic = false;
                astralBodyRigidbody.velocity    = velocity;
                astralBodyRigidbody.isKinematic = true;
            }

            velocityChangedEvent.Invoke(velocity);
        }

        public void ChangeVelocity(double realSpeed)
        {
            var speed = (float) (realSpeed /
                                 GameManager.GetGameManager
                                            .GetK(PropertyUnit.M)) * GameManager.GetGameManager
                                                                                .GetK(PropertyUnit.S);
            if (!astralBodyRigidbody.isKinematic)
            {
                astralBodyRigidbody.velocity = astralBodyRigidbody.velocity.normalized * speed;
            }
            else
            {
                _lastVelocity                   = _lastVelocity.normalized * speed;
                astralBodyRigidbody.isKinematic = false;
                astralBodyRigidbody.velocity    = _lastVelocity;
                astralBodyRigidbody.isKinematic = true;
            }
        }

        private protected virtual void SetMass()
        {
            // AstralBodyRigidbody.mass = Mathf.PI * (4/3)*Mathf.Pow(curSize,3) * this.density;
            astralBodyRigidbody.mass = Mass;
        }


        // public void ChangeDensity(float curDensity)
        // {
        //     density = curDensity;
        // }

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
                forceResult += astralBody.GetGravityVector3(astralBodyRigidbody);
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
            var core = affectedPlanets.OrderByDescending(a => GetGravityVector3(a.GetRigidbody()).magnitude)
                                      .FirstOrDefault();
            ChangeVelocity(CustomSolver.GetCircleOrbitVelocity(transform.position, core.GetTransform().position,
                                                               core._mass));
        }

        [ContextMenu("To Circle Velocity")]
        public void SetCircleVelocityMenu()
        {
            //查找引力核心
            var core = affectedPlanets.OrderByDescending(a => GetGravityVector3(a.GetRigidbody()).magnitude)
                                      .FirstOrDefault();
            oriVelocity = CustomSolver.GetCircleOrbitVelocity(transform.position, core.GetTransform().position, core._mass);
        }

        #endregion
    }
}