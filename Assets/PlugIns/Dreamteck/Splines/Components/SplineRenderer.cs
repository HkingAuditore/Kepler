using UnityEngine;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Spline Renderer")]
    [ExecuteInEditMode]
    public class SplineRenderer : MeshGenerator
    {
        [HideInInspector] public bool autoOrient = true;

        [HideInInspector] public int updateFrameInterval;


        [SerializeField] [HideInInspector] private int _slices = 1;

        [SerializeField] [HideInInspector] private Vector3 vertexDirection = Vector3.up;

        private int  currentFrame;
        private bool init;
        private bool orthographic;

        public int slices
        {
            get => _slices;
            set
            {
                if (value != _slices)
                {
                    if (value < 1) value = 1;
                    _slices = value;
                    Rebuild();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "spline";
        }

        private void Start()
        {
            if (Camera.current != null) orthographic = Camera.current.orthographic;
        }

        private void OnWillRenderObject()
        {
            if (!autoOrient) return;
            if (updateFrameInterval > 0)
                if (currentFrame != 0)
                    return;
            if (!Application.isPlaying)
                if (!init)
                {
                    Awake();
                    init = true;
                }

            RenderWithCamera(Camera.current);
        }

        protected override void LateRun()
        {
            if (updateFrameInterval > 0)
            {
                currentFrame++;
                if (currentFrame > updateFrameInterval) currentFrame = 0;
            }
        }

        protected override void BuildMesh()
        {
            base.BuildMesh();
            GenerateVertices(vertexDirection, orthographic);
            MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _slices, sampleCount, false);
        }

        public void RenderWithCamera(Camera cam)
        {
            orthographic = true;
            if (cam != null)
            {
                if (cam.orthographic) vertexDirection = -cam.transform.forward;
                else vertexDirection                  = cam.transform.position;
                orthographic = cam.orthographic;
            }

            BuildMesh();
            WriteMesh();
        }

        public void GenerateVertices(Vector3 vertexDirection, bool orthoGraphic)
        {
            AllocateMesh((_slices + 1) * sampleCount, _slices * (sampleCount - 1) * 6);
            var vertexIndex = 0;
            ResetUVDistance();
            var hasOffset = offset != Vector3.zero;
            for (var i = 0; i < sampleCount; i++)
            {
                GetSample(i, evalResult);
                var center = evalResult.position;
                if (hasOffset)
                    center += offset.x * -Vector3.Cross(evalResult.forward, evalResult.up) + offset.y * evalResult.up +
                              offset.z * evalResult.forward;
                Vector3 vertexNormal;
                if (orthoGraphic) vertexNormal = vertexDirection;
                else vertexNormal              = (vertexDirection - center).normalized;
                var vertexRight = Vector3.Cross(evalResult.forward, vertexNormal).normalized;
                if (uvMode == UVMode.UniformClamp || uvMode == UVMode.UniformClip) AddUVDistance(i);
                var vertexColor = evalResult.color * color;
                for (var n = 0; n < _slices + 1; n++)
                {
                    var slicePercent = (float) n / _slices;
                    tsMesh.vertices[vertexIndex] = center - vertexRight * evalResult.size * 0.5f * size +
                                                   vertexRight * evalResult.size * slicePercent * size;
                    CalculateUVs(evalResult.percent, slicePercent);
                    tsMesh.uv[vertexIndex] = Vector2.one * 0.5f +
                                             (Vector2) (Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
                                                        (Vector2.one * 0.5f - uvs));
                    tsMesh.normals[vertexIndex] = vertexNormal;
                    tsMesh.colors[vertexIndex]  = vertexColor;
                    vertexIndex++;
                }
            }
        }
    }
}