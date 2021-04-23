using System;
using System.Collections.Generic;
using System.Linq;
using GameManagers;
using Quiz;
using StaticClasses.MathPlus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SpacePhysic
{
    /// <summary>
    ///     星体物理实体
    /// </summary>
    public class AstralBody : MonoBehaviour, ITraceable
    {
        [SerializeField] private double _density;

        [FormerlySerializedAs("mass")] [Header("Basic Property")] [SerializeField]
        private float _mass;

        [SerializeField] private double _realMass;

        [FormerlySerializedAs("originalSize")] [SerializeField]
        private float _size = 1;

        /// <summary>
        ///     对之产生影响的星球
        /// </summary>
        public List<AstralBody> affectedPlanets = new List<AstralBody>();

        /// <summary>
        ///     引力影响范围
        /// </summary>
        public float affectRadius;

        /// <summary>
        ///     角速度
        /// </summary>
        public Vector3 angularVelocity;

        /// <summary>
        ///     屏蔽影响的星球
        /// </summary>
        public List<AstralBody> banAffectedPlanets = new List<AstralBody>();

        /// <summary>
        ///     物理碰撞检测盒
        /// </summary>
        public SphereCollider defaultCollider;

        /// <summary>
        ///     是否可以影响其他星球
        /// </summary>
        [Header("Gravity Property")] public bool enableAffect = true;

        /// <summary>
        ///     是否可以被追踪
        /// </summary>
        public bool enableTracing;

        /// <summary>
        ///     初速度
        /// </summary>
        [Header("Movement Property")] public Vector3 oriVelocity;

        /// <summary>
        ///     触发影响检测盒
        /// </summary>
        public SphereCollider triggerCollider;

        /// <summary>
        ///     速度变化事件
        /// </summary>
        public UnityEvent<Vector3> velocityChangedEvent;

        private Rigidbody _astralBodyRigidbody;
        private Vector3   _lastVelocity;

        private MeshFilter _mesh;
        [SerializeField]private int        _meshNum;
        private Renderer   _renderer;

        /// <summary>
        ///     质量（缩放后）
        /// </summary>
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

        /// <summary>
        ///     星体的刚体
        /// </summary>
        protected Rigidbody astralBodyRigidbody
        {
            get => _astralBodyRigidbody;
            set
            {
                if (_astralBodyRigidbody == null) _astralBodyRigidbody = GetComponent<Rigidbody>();
                _astralBodyRigidbody = value;
            }
        }

        /// <summary>
        ///     受力
        /// </summary>
        public Vector3 Force { get; private set; }

        /// <summary>
        ///     表面重力加速度
        /// </summary>
        public float gravity
        {
            get
            {
                double realSize = size * Mathf.Pow(10, GameManager.getGameManager.GetK(PropertyUnit.M));
                // Debug.Log("RealSize = " + realSize);
                return (float) (PhysicBase.GetRealG() * realMass / (realSize * realSize));
            }
        }

        /// <summary>
        ///     使用星球样式
        /// </summary>
        public int meshNum
        {
            get => _meshNum;
            set
            {
                _meshNum = value;
                var materials = new List<Material>();
                _mesh.sharedMesh = GameManager.getGameManager.GetMeshAndMaterialsFromList(_meshNum, ref materials);
                _renderer.sharedMaterials = materials.ToArray();
            }
        }

        /// <summary>
        ///     星球半径
        /// </summary>
        public float size
        {
            get => _size;
            set
            {
                _size = value;
                ChangeSize();
            }
        }

        /// <summary>
        ///     真实质量
        /// </summary>
        public double realMass
        {
            get => _realMass;
            set
            {
                _realMass = value;
                // Debug.Log(GameManager.GetGameManager.GetK(PropertyUnit.Kg));
                // Debug.Log( Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.Kg)));
                // Debug.Log("num:" + _realMass       * Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.Kg)));
                Mass = (float) (_realMass * Mathf.Pow(10, GameManager.getGameManager.GetK(PropertyUnit.Kg, _realMass)));
                // Debug.Log((_realMass * Mathf.Pow(10, GameManager.GetGameManager.GetK(PropertyUnit.Kg, _realMass))));
            }
        }

        /// <summary>
        ///     密度
        /// </summary>
        public double density
        {
            get
            {
                double realSize = size           * Mathf.Pow(10, GameManager.getGameManager.GetK(PropertyUnit.M));
                _density = (float) (3 * realMass / (4 * Mathf.PI * realSize * realSize * realSize));
                return _density;
            }
        }

        private Vector3 lastVelocity
        {
            get
            {
                // Debug.Log( this.name + ":" + _lastVelocity.magnitude);
                return _lastVelocity;
            }
            set
            {
                _lastVelocity = value;
                // Debug.Log(this.name + " Set velocity:" + value);
            }
        }
        
        public bool isLoadDone { get; protected set; } = false;


        private void Awake()
        {
            _mesh               = GetComponent<MeshFilter>();
            _renderer           = GetComponent<Renderer>();
            astralBodyRigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            Generate();
            isLoadDone = true;
        }

        protected void Generate()
        {
            triggerCollider.radius =  affectRadius;
            defaultCollider.radius *= 1.2f;
            SetMass();
            ChangeSize();

            astralBodyRigidbody.angularVelocity = angularVelocity;
            lastVelocity                        = oriVelocity;
            ChangeVelocity(oriVelocity);
        }

        protected virtual void FixedUpdate()
        {
            var force                                           = CalculateForce();
            if (!astralBodyRigidbody.isKinematic) lastVelocity = astralBodyRigidbody.velocity;
            //Debug.Log(this.name + " force: " + force);
            // Debug.DrawLine(transform.position,transform.position + force,Color.green);
            astralBodyRigidbody.AddForce(force);
        }

        private void OnDestroy()
        {
            GameManager.getGameManager.orbit.RemoveAstralBody(this);
            if (GameManager.getGameManager.sceneEditor != null)
                GameManager.getGameManager.sceneEditor.RemoveAstralBodyDict(this);
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

        /// <summary>
        ///     获取坐标
        /// </summary>
        /// <returns></returns>
        public Transform GetTransform()
        {
            return transform;
        }

        /// <summary>
        ///     委屈位置
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        /// <summary>
        ///     获取Gameobject
        /// </summary>
        /// <returns></returns>
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
            return lastVelocity;
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

        ///调整星球体积
        private void ChangeSize()
        {
            var localScale = GameManager.getGameManager.globalDistanceScaler <= 0
                ? 1
                : GameManager.getGameManager.globalDistanceScaler;
            var showSize = Mathf.Pow(size, .2f) * localScale * localScale;
            transform.localScale = new Vector3(showSize, showSize, showSize);
            // defaultCollider.radius *= size;
            // if(enableAffect)
            //     triggerCollider.radius = affectRadius * size;
            // size = curSize;
        }


        /// <summary>
        ///     调整星球速度
        /// </summary>
        /// <param name="velocity">速度向量</param>
        public void ChangeVelocity(Vector3 velocity)
        {
            if (!astralBodyRigidbody.isKinematic)
            {
                astralBodyRigidbody.velocity = velocity;
            }
            else
            {
                lastVelocity                   = velocity;
                astralBodyRigidbody.isKinematic = false;
                astralBodyRigidbody.velocity    = velocity;
                astralBodyRigidbody.isKinematic = true;
            }

            velocityChangedEvent.Invoke(velocity);
        }

        /// <summary>
        ///     保持当前速度方向调整速度
        /// </summary>
        /// <param name="realSpeed">新速度数值</param>
        public void ChangeVelocity(double realSpeed)
        {
            var speed = (float) (realSpeed /
                                 GameManager.getGameManager
                                            .GetK(PropertyUnit.M)) * GameManager.getGameManager
                                                                                .GetK(PropertyUnit.S);
            if (!astralBodyRigidbody.isKinematic)
            {
                astralBodyRigidbody.velocity = astralBodyRigidbody.velocity.normalized * speed;
            }
            else
            {
                lastVelocity                   = lastVelocity.normalized * speed;
                astralBodyRigidbody.isKinematic = false;
                astralBodyRigidbody.velocity    = lastVelocity;
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

        private Vector3 GetGravityVector3(Rigidbody rigidbody)
        {
            var distance            = Vector3.Distance(transform.position, rigidbody.position);
            var normalizedDirection = (transform.position - rigidbody.position).normalized;
            return CalculateGravityModulus(rigidbody.mass, distance) * normalizedDirection;
        }

        /// <summary>
        ///     计算受力
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     设置环绕速度
        /// </summary>
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
            oriVelocity =
                CustomSolver.GetCircleOrbitVelocity(transform.position, core.GetTransform().position, core._mass);
        }

        #endregion
    }
}