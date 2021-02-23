using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Dreamteck.Splines;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Spline Renderer")]
    [ExecuteInEditMode]
    public class SplineRenderer : MeshGenerator
    {
        public int slices
        {
            get { return _slices; }
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
        [HideInInspector]
        public bool autoOrient = true;
        [HideInInspector]
        public int updateFrameInterval = 0;

        private int currentFrame = 0;


        [SerializeField]
        [HideInInspector]
        private int _slices = 1;
        [SerializeField]
        [HideInInspector]
        private Vector3 vertexDirection = Vector3.up;
        private bool orthographic = false;
        private bool init = false;

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "spline";
        }

        void Start()
        {
            if (Camera.current != null) orthographic = Camera.current.orthographic;
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
            MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _slices, sampleCount, false, 0, 0);
        }

        public void RenderWithCamera(Camera cam)
        {
            orthographic = true;
            if (cam != null)
            {
                if (cam.orthographic) vertexDirection = -cam.transform.forward;
                else vertexDirection = cam.transform.position;
                orthographic = cam.orthographic;
            }
            BuildMesh();
            WriteMesh();
        }

        void OnWillRenderObject()
        {
            if (!autoOrient) return;
            if (updateFrameInterval > 0)
            {
                if (currentFrame != 0) return;
            }
            if (!Application.isPlaying)
            {
                if (!init)
                {
                    Awake();
                    init = true;
                }
            }
            RenderWithCamera(Camera.current);
        }

        public void GenerateVertices(Vector3 vertexDirection, bool orthoGraphic)
        {
            AllocateMesh((_slices + 1) * sampleCount, _slices * (sampleCount - 1) * 6);
            int vertexIndex = 0;
            ResetUVDistance();
            bool hasOffset = offset != Vector3.zero;
            for (int i = 0; i < sampleCount; i++)
            {
                GetSample(i, evalResult);
                Vector3 center = evalResult.position;
                if (hasOffset) center += offset.x * -Vector3.Cross(evalResult.forward, evalResult.up) + offset.y * evalResult.up + offset.z * evalResult.forward;
                Vector3 vertexNormal;
                if(orthoGraphic) vertexNormal = vertexDirection;
                else vertexNormal = (vertexDirection - center).normalized;
                Vector3 vertexRight = Vector3.Cross(evalResult.forward, vertexNormal).normalized;
                if (uvMode == UVMode.UniformClamp || uvMode == UVMode.UniformClip) AddUVDistance(i);
                Color vertexColor = evalResult.color * color;
                for (int n = 0; n < _slices + 1; n++)
                {
                    float slicePercent = ((float)n / _slices);
                    tsMesh.vertices[vertexIndex] = center - vertexRight * evalResult.size * 0.5f * size + vertexRight * evalResult.size * slicePercent * size;
                    CalculateUVs(evalResult.percent, slicePercent);
                    tsMesh.uv[vertexIndex] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - uvs));
                    tsMesh.normals[vertexIndex] = vertexNormal;
                    tsMesh.colors[vertexIndex] = vertexColor;
                    vertexIndex++;
                }
            }
        }


    }
}
