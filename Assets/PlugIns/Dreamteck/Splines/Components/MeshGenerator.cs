using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Dreamteck.Splines
{
    public class MeshGenerator : SplineUser
    {
        public enum NormalMethod
        {
            Recalculate,
            SplineNormals
        }


        public enum UVMode
        {
            Clip,
            UniformClip,
            Clamp,
            UniformClamp
        }

        protected const  int     UNITY_VERTEX_LIMIT = 65534;
        protected static Vector2 uvs                = Vector2.zero;

        [SerializeField] [HideInInspector] private bool _baked;

        [SerializeField] [HideInInspector] private bool _markDynamic = true;

        [SerializeField] [HideInInspector] private float _size = 1f;

        [SerializeField] [HideInInspector] private Color _color = Color.white;

        [SerializeField] [HideInInspector] private Vector3 _offset = Vector3.zero;

        [SerializeField] [HideInInspector] private NormalMethod _normalMethod = NormalMethod.SplineNormals;

        [SerializeField] [HideInInspector] private bool _calculateTangents = true;

        [SerializeField] [HideInInspector] private bool _useSplineSize = true;

        [SerializeField] [HideInInspector] private bool _useSplineColor = true;

        [SerializeField] [HideInInspector] [Range(-360f, 360f)]
        private float _rotation;

        [SerializeField] [HideInInspector] private bool _flipFaces;

        [SerializeField] [HideInInspector] private bool _doubleSided;

        [SerializeField] [HideInInspector] private UVMode _uvMode = UVMode.Clip;

        [SerializeField] [HideInInspector] private Vector2 _uvScale = Vector2.one;

        [SerializeField] [HideInInspector] private Vector2 _uvOffset = Vector2.zero;

        [SerializeField] [HideInInspector] private float _uvRotation;

        [SerializeField] [HideInInspector] protected MeshCollider meshCollider;

        [SerializeField] [HideInInspector] protected MeshFilter filter;

        [SerializeField] [HideInInspector] protected MeshRenderer meshRenderer;

        [SerializeField] [HideInInspector] protected Mesh mesh;

        [HideInInspector] public float colliderUpdateRate = 0.2f;

        protected float lastUpdateTime;

        [SerializeField] [HideInInspector] protected TS_Mesh tsMesh = new TS_Mesh();

        protected bool updateCollider;

        private float vDist;

        public float size
        {
            get => _size;
            set
            {
                if (value != _size)
                {
                    _size = value;
                    Rebuild();
                }
                else
                {
                    _size = value;
                }
            }
        }

        public Color color
        {
            get => _color;
            set
            {
                if (value != _color)
                {
                    _color = value;
                    Rebuild();
                }
            }
        }

        public Vector3 offset
        {
            get => _offset;
            set
            {
                if (value != _offset)
                {
                    _offset = value;
                    Rebuild();
                }
            }
        }

        public NormalMethod normalMethod
        {
            get => _normalMethod;
            set
            {
                if (value != _normalMethod)
                {
                    _normalMethod = value;
                    Rebuild();
                }
            }
        }

        public bool useSplineSize
        {
            get => _useSplineSize;
            set
            {
                if (value != _useSplineSize)
                {
                    _useSplineSize = value;
                    Rebuild();
                }
            }
        }

        public bool useSplineColor
        {
            get => _useSplineColor;
            set
            {
                if (value != _useSplineColor)
                {
                    _useSplineColor = value;
                    Rebuild();
                }
            }
        }

        public bool calculateTangents
        {
            get => _calculateTangents;
            set
            {
                if (value != _calculateTangents)
                {
                    _calculateTangents = value;
                    Rebuild();
                }
            }
        }

        public float rotation
        {
            get => _rotation;
            set
            {
                if (value != _rotation)
                {
                    _rotation = value;
                    Rebuild();
                }
            }
        }

        public bool flipFaces
        {
            get => _flipFaces;
            set
            {
                if (value != _flipFaces)
                {
                    _flipFaces = value;
                    Rebuild();
                }
            }
        }

        public bool doubleSided
        {
            get => _doubleSided;
            set
            {
                if (value != _doubleSided)
                {
                    _doubleSided = value;
                    Rebuild();
                }
            }
        }

        public UVMode uvMode
        {
            get => _uvMode;
            set
            {
                if (value != _uvMode)
                {
                    _uvMode = value;
                    Rebuild();
                }
            }
        }

        public Vector2 uvScale
        {
            get => _uvScale;
            set
            {
                if (value != _uvScale)
                {
                    _uvScale = value;
                    Rebuild();
                }
            }
        }

        public Vector2 uvOffset
        {
            get => _uvOffset;
            set
            {
                if (value != _uvOffset)
                {
                    _uvOffset = value;
                    Rebuild();
                }
            }
        }

        public float uvRotation
        {
            get => _uvRotation;
            set
            {
                if (value != _uvRotation)
                {
                    _uvRotation = value;
                    Rebuild();
                }
            }
        }

        public bool baked => _baked;

        public bool markDynamic
        {
            get => _markDynamic;
            set
            {
                if (value != _markDynamic)
                {
                    _markDynamic = value;
                    if (!_markDynamic)
                    {
                        Destroy(mesh);
                        mesh = new Mesh();
                    }

                    RebuildImmediate();
                }
            }
        }

        protected override void Awake()
        {
            if (mesh == null) mesh = new Mesh();
            base.Awake();
            filter       = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        protected override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            var rend          = GetComponent<MeshRenderer>();
            var materialFound = false;
            for (var i = 0; i < rend.sharedMaterials.Length; i++)
                if (rend.sharedMaterials[i] != null)
                {
                    materialFound = true;
                    break;
                }

            if (!materialFound)
                rend.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            var filter                           = GetComponent<MeshFilter>();
            var rend                             = GetComponent<MeshRenderer>();
            if (filter != null) filter.hideFlags = HideFlags.None;
            if (rend   != null) rend.hideFlags   = HideFlags.None;
        }

        public void CloneMesh()
        {
            if (tsMesh != null)
                tsMesh = TS_Mesh.Copy(tsMesh);
            else
                tsMesh = new TS_Mesh();
            if (mesh != null)
                mesh = Instantiate(mesh);
            else
                mesh = new Mesh();
        }

        public override void Rebuild()
        {
            if (_baked) return;
            base.Rebuild();
        }

        public override void RebuildImmediate()
        {
            if (_baked) return;
            base.RebuildImmediate();
        }


        public void UpdateCollider()
        {
            meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
        }

        protected override void LateRun()
        {
            if (_baked) return;
            base.LateRun();
            if (updateCollider)
                if (meshCollider != null)
                    if (Time.time - lastUpdateTime >= colliderUpdateRate)
                    {
                        lastUpdateTime          = Time.time;
                        updateCollider          = false;
                        meshCollider.sharedMesh = filter.sharedMesh;
                    }
        }

        protected override void Build()
        {
            base.Build();
            BuildMesh();
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            WriteMesh();
        }

        protected virtual void BuildMesh()
        {
            //Logic for mesh generation, automatically called in the Build method
        }

        protected virtual void WriteMesh()
        {
            MeshUtility.InverseTransformMesh(tsMesh, trs);
            if (_doubleSided)
                MeshUtility.MakeDoublesidedHalf(tsMesh);
            else if (_flipFaces) MeshUtility.FlipFaces(tsMesh);

            if (_calculateTangents) MeshUtility.CalculateTangents(tsMesh);

            if (tsMesh.vertexCount > UNITY_VERTEX_LIMIT)
                Debug.LogError("WARNING: The generated mesh for " + name + " has " + tsMesh.vertexCount +
                               " vertices. The maximum vertex count for meshes in Unity is " + UNITY_VERTEX_LIMIT +
                               ". The mesh will not be updated.");

            if (_markDynamic) mesh.MarkDynamic();

            tsMesh.WriteMesh(ref mesh);
            if (_normalMethod == 0) mesh.RecalculateNormals();
            if (filter        != null) filter.sharedMesh = mesh;
            updateCollider = true;
        }

        protected virtual void AllocateMesh(int vertexCount, int trisCount)
        {
            if (trisCount   < 0) trisCount   = 0;
            if (vertexCount < 0) vertexCount = 0;
            if (_doubleSided)
            {
                vertexCount *= 2;
                trisCount   *= 2;
            }

            if (tsMesh.vertexCount != vertexCount)
            {
                tsMesh.vertices = new Vector3[vertexCount];
                tsMesh.normals  = new Vector3[vertexCount];
                tsMesh.tangents = new Vector4[vertexCount];
                tsMesh.colors   = new Color[vertexCount];
                tsMesh.uv       = new Vector2[vertexCount];
            }

            if (tsMesh.triangles.Length != trisCount) tsMesh.triangles = new int[trisCount];
        }

        protected void ResetUVDistance()
        {
            vDist = 0f;
            if (uvMode == UVMode.UniformClip) vDist = spline.CalculateLength(0.0, GetSampleRaw(0).percent);
        }

        protected void AddUVDistance(int sampleIndex)
        {
            if (sampleIndex == 0) return;
            vDist += Vector3.Distance(GetSampleRaw(sampleIndex).position, GetSampleRaw(sampleIndex - 1).position);
        }

        protected void CalculateUVs(double percent, float u)
        {
            uvs.x = u * _uvScale.x - _uvOffset.x;
            switch (uvMode)
            {
                case UVMode.Clip:
                    uvs.y = (float) percent * _uvScale.y - _uvOffset.y;
                    break;
                case UVMode.Clamp:
                    uvs.y = (float) DMath.InverseLerp(clipFrom, clipTo, percent) * _uvScale.y - _uvOffset.y;
                    break;
                case UVMode.UniformClamp:
                    uvs.y = vDist * _uvScale.y / (float) span - _uvOffset.y;
                    break;
                default:
                    uvs.y = vDist * _uvScale.y - _uvOffset.y;
                    break;
            }
        }

        protected float GetBaseSize(SplineSample sample)
        {
            return _useSplineSize ? sample.size : 1f;
        }

        protected Color GetBaseColor(SplineSample sample)
        {
            return _useSplineColor ? sample.color : Color.white;
        }

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            base.EditorAwake();
            CloneMesh();
            Awake();
        }

        public void Bake(bool makeStatic, bool lightmapUV)
        {
            if (mesh == null) return;
            gameObject.isStatic = false;
            UnityEditor.MeshUtility.Optimize(mesh);
            if (spline != null) spline.Unsubscribe(this);
            filter            = GetComponent<MeshFilter>();
            meshRenderer      = GetComponent<MeshRenderer>();
            filter.hideFlags  = meshRenderer.hideFlags = HideFlags.None;
            filter.sharedMesh = mesh;
            if (lightmapUV) Unwrapping.GenerateSecondaryUVSet(filter.sharedMesh);
            if (makeStatic) gameObject.isStatic = true;
            _baked = true;
        }

        public void Unbake()
        {
            gameObject.isStatic = false;
            _baked              = false;
            spline.Subscribe(this);
            Rebuild();
        }
#endif
    }
}