using UnityEngine;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Waveform Generator")]
    public class WaveformGenerator : MeshGenerator
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public enum Space
        {
            World,
            Local
        }

        public enum UVWrapMode
        {
            Clamp,
            UniformX,
            UniformY,
            Uniform
        }

        [SerializeField] [HideInInspector] private Axis _axis = Axis.Y;

        [SerializeField] [HideInInspector] private bool _symmetry;

        [SerializeField] [HideInInspector] private UVWrapMode _uvWrapMode = UVWrapMode.Clamp;

        [SerializeField] [HideInInspector] private int _slices = 1;

        public Axis axis
        {
            get => _axis;
            set
            {
                if (value != _axis)
                {
                    _axis = value;
                    Rebuild();
                }
            }
        }

        public bool symmetry
        {
            get => _symmetry;
            set
            {
                if (value != _symmetry)
                {
                    _symmetry = value;
                    Rebuild();
                }
            }
        }

        public UVWrapMode uvWrapMode
        {
            get => _uvWrapMode;
            set
            {
                if (value != _uvWrapMode)
                {
                    _uvWrapMode = value;
                    Rebuild();
                }
            }
        }

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
            mesh.name = "waveform";
        }

        protected override void BuildMesh()
        {
            base.BuildMesh();
            Generate();
        }

        protected override void Build()
        {
            base.Build();
        }

        protected override void LateRun()
        {
            base.LateRun();
        }

        private void Generate()
        {
            var vertexCount = sampleCount     * (_slices     + 1);
            AllocateMesh(vertexCount, _slices * (sampleCount - 1) * 6);
            var vertIndex        = 0;
            var avgTop           = 0f;
            var totalLength      = 0f;
            var computerPosition = spline.position;
            var normal           = spline.TransformDirection(Vector3.right);
            switch (_axis)
            {
                case Axis.Y:
                    normal = spline.TransformDirection(Vector3.up);
                    break;
                case Axis.Z:
                    normal = spline.TransformDirection(Vector3.forward);
                    break;
            }

            for (var i = 0; i < sampleCount; i++)
            {
                evalResult = GetSampleRaw(i);
                var resultSize          = GetBaseSize(evalResult);
                var samplePosition      = evalResult.position;
                var localSamplePosition = spline.InverseTransformPoint(samplePosition);
                var bottomPosition      = localSamplePosition;
                var sampleDirection     = evalResult.forward;
                var sampleNormal        = evalResult.up;

                var heightPercent = 1f;
                if (_uvWrapMode == UVWrapMode.UniformX || _uvWrapMode == UVWrapMode.Uniform)
                    if (i > 0)
                        totalLength += Vector3.Distance(evalResult.position, GetSampleRaw(i - 1).position);
                switch (_axis)
                {
                    case Axis.X:
                        bottomPosition.x =  _symmetry ? -localSamplePosition.x : 0f;
                        heightPercent    =  uvScale.y * Mathf.Abs(localSamplePosition.x);
                        avgTop           += localSamplePosition.x;
                        break;
                    case Axis.Y:
                        bottomPosition.y =  _symmetry ? -localSamplePosition.y : 0f;
                        heightPercent    =  uvScale.y * Mathf.Abs(localSamplePosition.y);
                        avgTop           += localSamplePosition.y;
                        break;
                    case Axis.Z:
                        bottomPosition.z =  _symmetry ? -localSamplePosition.z : 0f;
                        heightPercent    =  uvScale.y * Mathf.Abs(localSamplePosition.z);
                        avgTop           += localSamplePosition.z;
                        break;
                }

                bottomPosition = spline.TransformPoint(bottomPosition);
                var right       = Vector3.Cross(normal,       sampleDirection).normalized;
                var offsetRight = Vector3.Cross(sampleNormal, sampleDirection);

                for (var n = 0; n < _slices + 1; n++)
                {
                    var slicePercent = (float) n / _slices;
                    tsMesh.vertices[vertIndex] = Vector3.Lerp(bottomPosition, samplePosition, slicePercent) +
                                                 normal      * (offset.y * resultSize)                      +
                                                 offsetRight * (offset.x * resultSize);
                    tsMesh.normals[vertIndex] = right;
                    switch (_uvWrapMode)
                    {
                        case UVWrapMode.Clamp:
                            tsMesh.uv[vertIndex] = new Vector2((float) evalResult.percent * uvScale.x + uvOffset.x,
                                                               slicePercent               * uvScale.y + uvOffset.y);
                            break;
                        case UVWrapMode.UniformX:
                            tsMesh.uv[vertIndex] = new Vector2(totalLength  * uvScale.x + uvOffset.x,
                                                               slicePercent * uvScale.y + uvOffset.y);
                            break;
                        case UVWrapMode.UniformY:
                            tsMesh.uv[vertIndex] = new Vector2((float) evalResult.percent * uvScale.x + uvOffset.x,
                                                               heightPercent * slicePercent * uvScale.y + uvOffset.y);
                            break;
                        case UVWrapMode.Uniform:
                            tsMesh.uv[vertIndex] = new Vector2(totalLength   * uvScale.x                + uvOffset.x,
                                                               heightPercent * slicePercent * uvScale.y + uvOffset.y);
                            break;
                    }

                    tsMesh.colors[vertIndex] = GetBaseColor(evalResult) * color;
                    vertIndex++;
                }
            }

            if (sampleCount > 0) avgTop /= sampleCount;
            MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _slices, sampleCount, avgTop < 0f);
        }
    }
}