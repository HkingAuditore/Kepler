using UnityEngine;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Tube Generator")]
    public class TubeGenerator : MeshGenerator
    {
        public enum CapMethod
        {
            None,
            Flat,
            Round
        }

        [SerializeField] [HideInInspector] private int _sides = 12;

        [SerializeField] [HideInInspector] private int _roundCapLatitude = 6;

        [SerializeField] [HideInInspector] private CapMethod _capMode = CapMethod.None;

        [SerializeField] [HideInInspector] [Range(0f, 360f)]
        private float _revolve = 360f;

        [SerializeField] [HideInInspector] private float _capUVScale = 1f;

        [SerializeField] [HideInInspector] private float _uvTwist;

        private int bodyTrisCount;

        private int bodyVertexCount;
        private int capTrisCount;
        private int capVertexCount;

        public int sides
        {
            get => _sides;
            set
            {
                if (value != _sides)
                {
                    if (value < 3) value = 3;
                    _sides = value;
                    Rebuild();
                }
            }
        }

        public CapMethod capMode
        {
            get => _capMode;
            set
            {
                if (value != _capMode)
                {
                    _capMode = value;
                    Rebuild();
                }
            }
        }

        public int roundCapLatitude
        {
            get => _roundCapLatitude;
            set
            {
                if (value < 1) value = 1;
                if (value != _roundCapLatitude)
                {
                    _roundCapLatitude = value;
                    if (_capMode == CapMethod.Round) Rebuild();
                }
            }
        }

        public float revolve
        {
            get => _revolve;
            set
            {
                if (value != _revolve)
                {
                    _revolve = value;
                    Rebuild();
                }
            }
        }

        public float capUVScale
        {
            get => _capUVScale;
            set
            {
                if (value != _capUVScale)
                {
                    _capUVScale = value;
                    Rebuild();
                }
            }
        }

        public float uvTwist
        {
            get => _uvTwist;
            set
            {
                if (value != _uvTwist)
                {
                    _uvTwist = value;
                    Rebuild();
                }
            }
        }

        private bool useCap
        {
            get
            {
                var isCapSet = _capMode != CapMethod.None;
                if (spline != null) return isCapSet && (!spline.isClosed || span < 1f);
                return isCapSet;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "tube";
        }

        protected override void Reset()
        {
            base.Reset();
        }


        protected override void BuildMesh()
        {
            if (_sides <= 2) return;
            base.BuildMesh();
            bodyVertexCount = (_sides + 1) * sampleCount;
            var _capModeFinal          = _capMode;
            if (!useCap) _capModeFinal = CapMethod.None;
            switch (_capModeFinal)
            {
                case CapMethod.Flat:
                    capVertexCount = _sides + 1;
                    break;
                case CapMethod.Round:
                    capVertexCount = _roundCapLatitude * (sides + 1);
                    break;
                default:
                    capVertexCount = 0;
                    break;
            }

            var vertexCount = bodyVertexCount + capVertexCount * 2;

            bodyTrisCount = _sides * (sampleCount - 1) * 2 * 3;
            switch (_capModeFinal)
            {
                case CapMethod.Flat:
                    capTrisCount = (_sides - 1) * 3 * 2;
                    break;
                case CapMethod.Round:
                    capTrisCount = _sides * _roundCapLatitude * 6;
                    break;
                default:
                    capTrisCount = 0;
                    break;
            }

            AllocateMesh(vertexCount, bodyTrisCount + capTrisCount * 2);

            Generate();
            switch (_capModeFinal)
            {
                case CapMethod.Flat:
                    GenerateFlatCaps();
                    break;
                case CapMethod.Round:
                    GenerateRoundCaps();
                    break;
            }
        }

        private void Generate()
        {
            var vertexIndex = 0;
            ResetUVDistance();
            var hasOffset = offset != Vector3.zero;
            for (var i = 0; i < sampleCount; i++)
            {
                GetSample(i, evalResult);
                var center     = evalResult.position;
                var right      = evalResult.right;
                var resultSize = GetBaseSize(evalResult);
                if (hasOffset)
                    center += offset.x * resultSize * right + offset.y * resultSize * evalResult.up +
                              offset.z * resultSize * evalResult.forward;
                if (uvMode == UVMode.UniformClamp || uvMode == UVMode.UniformClip) AddUVDistance(i);
                var vertexColor = GetBaseColor(evalResult) * color;
                for (var n = 0; n < _sides + 1; n++)
                {
                    var anglePercent = (float) n / _sides;
                    var rot = Quaternion.AngleAxis(_revolve * anglePercent + rotation + 180f, evalResult.forward);
                    tsMesh.vertices[vertexIndex] = center + rot * right * (size * resultSize * 0.5f);
                    CalculateUVs(evalResult.percent, anglePercent);
                    tsMesh.uv[vertexIndex] = Vector2.one * 0.5f +
                                             (Vector2) (Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
                                                        (Vector2.one * 0.5f - (uvs + Vector2.right *
                                                            ((float) evalResult.percent * _uvTwist))));
                    tsMesh.normals[vertexIndex] = Vector3.Normalize(tsMesh.vertices[vertexIndex] - center);
                    tsMesh.colors[vertexIndex]  = vertexColor;
                    vertexIndex++;
                }
            }

            MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _sides, sampleCount, false);
        }

        private void GenerateFlatCaps()
        {
            //Start Cap

            GetSample(0, evalResult);
            for (var i = 0; i < _sides + 1; i++)
            {
                var index = bodyVertexCount + i;
                tsMesh.vertices[index] = tsMesh.vertices[i];
                tsMesh.normals[index]  = -evalResult.forward;
                tsMesh.colors[index]   = tsMesh.colors[i];
                tsMesh.uv[index] =
                    Quaternion.AngleAxis(_revolve * ((float) i / (_sides - 1)), Vector3.forward) * Vector2.right *
                    (0.5f * capUVScale) + Vector3.right * 0.5f + Vector3.up * 0.5f;
            }

            //End Cap
            GetSample(sampleCount - 1, evalResult);
            for (var i = 0; i < _sides + 1; i++)
            {
                var index = bodyVertexCount + _sides + 1 + i;
                var bodyIndex = bodyVertexCount - (_sides + 1) + i;
                tsMesh.vertices[index] = tsMesh.vertices[bodyIndex];
                tsMesh.normals[index] = GetSampleRaw(sampleCount - 1).forward;
                tsMesh.colors[index] = tsMesh.colors[bodyIndex];
                tsMesh.uv[index] =
                    Quaternion.AngleAxis(_revolve * ((float) bodyIndex / (_sides - 1)), Vector3.forward) *
                    Vector2.right * (0.5f * capUVScale) + Vector3.right * 0.5f + Vector3.up * 0.5f;
            }

            var t             = bodyTrisCount;
            var fullIntegrity = _revolve == 360f;
            var finalSides    = fullIntegrity ? _sides - 1 : _sides;
            //Start cap
            for (var i = 0; i < finalSides - 1; i++)
            {
                tsMesh.triangles[t++] = i + bodyVertexCount  + 2;
                tsMesh.triangles[t++] = i + +bodyVertexCount + 1;
                tsMesh.triangles[t++] = bodyVertexCount;
            }

            //End cap
            for (var i = 0; i < finalSides - 1; i++)
            {
                tsMesh.triangles[t++] = bodyVertexCount + _sides + 1;
                tsMesh.triangles[t++] = i               + 1      + bodyVertexCount + _sides + 1;
                tsMesh.triangles[t++] = i               + 2      + bodyVertexCount + _sides + 1;
            }
        }

        private void GenerateRoundCaps()
        {
            //Start Cap
            GetSample(0, evalResult);
            var center     = evalResult.position;
            var hasOffset  = offset != Vector3.zero;
            var resultSize = GetBaseSize(evalResult);
            if (hasOffset)
                center += offset.x * resultSize * evalResult.right + offset.y * resultSize * evalResult.up +
                          offset.z * resultSize * evalResult.forward;
            var lookRot          = Quaternion.LookRotation(-evalResult.forward, evalResult.up);
            var startV           = 0f;
            var capLengthPercent = 0f;
            switch (uvMode)
            {
                case UVMode.Clip:
                    startV           = (float) evalResult.percent;
                    capLengthPercent = size * 0.5f / spline.CalculateLength();
                    break;
                case UVMode.UniformClip:
                    startV           = spline.CalculateLength(0.0, evalResult.percent);
                    capLengthPercent = size * 0.5f;
                    break;
                case UVMode.UniformClamp:
                    startV           = 0f;
                    capLengthPercent = size * 0.5f / (float) span;
                    break;
                case UVMode.Clamp:
                    capLengthPercent = size * 0.5f / spline.CalculateLength(clipFrom, clipTo);
                    break;
            }

            var vertexColor = GetBaseColor(evalResult) * color;
            for (var lat = 1; lat < _roundCapLatitude + 1; lat++)
            {
                var latitudePercent = (float) lat / _roundCapLatitude;
                var latAngle        = 90f         * latitudePercent;
                for (var lon = 0; lon <= sides; lon++)
                {
                    var anglePercent = (float) lon / sides;
                    var index = bodyVertexCount + lon + (lat - 1) * (sides + 1);
                    var rot = Quaternion.AngleAxis(_revolve * anglePercent + rotation + 180f, -Vector3.forward) *
                              Quaternion.AngleAxis(latAngle, Vector3.up);
                    tsMesh.vertices[index] = center + lookRot * rot * -Vector3.right * (size * 0.5f * evalResult.size);
                    tsMesh.colors[index]   = vertexColor;
                    tsMesh.normals[index]  = (tsMesh.vertices[index] - center).normalized;
                    var baseV  = startV + capLengthPercent * latitudePercent;
                    var baseUV = new Vector2(anglePercent * uvScale.x - baseV * _uvTwist, baseV * uvScale.y) - uvOffset;
                    tsMesh.uv[index] = Vector2.one * 0.5f +
                                       (Vector2) (Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
                                                  (Vector2.one * 0.5f - baseUV));
                }
            }


            //Triangles
            var t = bodyTrisCount;
            for (var z = -1; z < _roundCapLatitude - 1; z++)
            {
                for (var x = 0; x < sides; x++)
                {
                    var current = bodyVertexCount + x     + z * (sides + 1);
                    var next    = current         + sides + 1;
                    if (z == -1)
                    {
                        current = x;
                        next    = bodyVertexCount + x;
                    }

                    tsMesh.triangles[t++] = next    + 1;
                    tsMesh.triangles[t++] = current + 1;
                    tsMesh.triangles[t++] = current;
                    tsMesh.triangles[t++] = next;
                    tsMesh.triangles[t++] = next + 1;
                    tsMesh.triangles[t++] = current;
                }
            }


            //End Cap
            GetSample(sampleCount - 1, evalResult);
            center     = evalResult.position;
            resultSize = GetBaseSize(evalResult);
            if (hasOffset)
                center += offset.x * resultSize * evalResult.right + offset.y * resultSize * evalResult.up +
                          offset.z * resultSize * evalResult.forward;
            lookRot = Quaternion.LookRotation(evalResult.forward, evalResult.up);
            switch (uvMode)
            {
                case UVMode.Clip:
                    startV = (float) evalResult.percent;
                    break;
                case UVMode.UniformClip:
                    startV = spline.CalculateLength(0.0, evalResult.percent);
                    break;
                case UVMode.Clamp:
                    startV = 1f;
                    break;
                case UVMode.UniformClamp:
                    startV = spline.CalculateLength();
                    break;
            }

            vertexColor = GetBaseColor(evalResult) * color;
            for (var lat = 1; lat < _roundCapLatitude + 1; lat++)
            {
                var latitudePercent = (float) lat / _roundCapLatitude;
                var latAngle        = 90f         * latitudePercent;
                for (var lon = 0; lon <= sides; lon++)
                {
                    var anglePercent = (float) lon / sides;
                    var index = bodyVertexCount + capVertexCount + lon + (lat - 1) * (sides + 1);
                    var rot = Quaternion.AngleAxis(_revolve * anglePercent + rotation + 180f, Vector3.forward) *
                              Quaternion.AngleAxis(latAngle, -Vector3.up);
                    tsMesh.vertices[index] = center + lookRot * rot * Vector3.right * size * 0.5f * evalResult.size;
                    tsMesh.normals[index] = (tsMesh.vertices[index] - center).normalized;
                    tsMesh.colors[index] = vertexColor;
                    var baseV  = startV + capLengthPercent * latitudePercent;
                    var baseUV = new Vector2(anglePercent * uvScale.x + baseV * _uvTwist, baseV * uvScale.y) - uvOffset;
                    tsMesh.uv[index] = Vector2.one * 0.5f +
                                       (Vector2) (Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
                                                  (Vector2.one * 0.5f - baseUV));
                }
            }

            //Triangles
            for (var z = -1; z < _roundCapLatitude - 1; z++)
            {
                for (var x = 0; x < sides; x++)
                {
                    var current = bodyVertexCount + capVertexCount + x + z * (sides + 1);
                    var next    = current         + sides          + 1;
                    if (z == -1)
                    {
                        current = bodyVertexCount - (_sides + 1) + x;
                        next    = bodyVertexCount                + capVertexCount + x;
                    }

                    tsMesh.triangles[t++] = current + 1;
                    tsMesh.triangles[t++] = next    + 1;
                    tsMesh.triangles[t++] = next;
                    tsMesh.triangles[t++] = next;
                    tsMesh.triangles[t++] = current;
                    tsMesh.triangles[t++] = current + 1;
                }
            }
        }
    }
}