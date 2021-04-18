using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Spline Mesh")]
    public class SplineMesh : MeshGenerator
    {
        //Mesh data
        [SerializeField] [HideInInspector] private List<Channel> channels = new List<Channel>();

        private readonly List<TS_Mesh> combineMeshes  = new List<TS_Mesh>();
        private readonly SplineSample  lastResult     = new SplineSample();
        private readonly SplineSample  modifiedResult = new SplineSample();
        private          int           meshCount;
        private          Matrix4x4     normalMatrix;
        private          bool          useLastResult;

        private Matrix4x4 vertexMatrix;

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "Extruded Mesh";
        }

        protected override void Reset()
        {
            base.Reset();
            AddChannel("Channel 1");
        }

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            for (var i = 0; i < channels.Count; i++)
            {
                for (var j = 0; j < channels[i].GetMeshCount(); j++) channels[i].GetMesh(j).Refresh();
            }

            base.EditorAwake();
        }
#endif

        public void RemoveChannel(int index)
        {
            channels.RemoveAt(index);
            Rebuild();
        }

        public void SwapChannels(int a, int b)
        {
            if (a < 0 || a >= channels.Count || b < 0 || b >= channels.Count) return;
            var temp = channels[b];
            channels[b] = channels[a];
            channels[a] = temp;
            Rebuild();
        }

        public Channel AddChannel(Mesh inputMesh, string name)
        {
            var channel = new Channel(name, inputMesh, this);
            channels.Add(channel);
            return channel;
        }

        public Channel AddChannel(string name)
        {
            var channel = new Channel(name, this);
            channels.Add(channel);
            return channel;
        }

        public int GetChannelCount()
        {
            return channels.Count;
        }

        public Channel GetChannel(int index)
        {
            return channels[index];
        }


        protected override void BuildMesh()
        {
            base.BuildMesh();
            Generate();
        }

        private void Generate()
        {
            meshCount = 0;
            for (var i = 0; i < channels.Count; i++)
            {
                if (channels[i].GetMeshCount() == 0) continue;
                meshCount += channels[i].count;
            }

            if (meshCount == 0)
            {
                tsMesh.Clear();
                return;
            }

            if (combineMeshes.Count < meshCount) combineMeshes.AddRange(new TS_Mesh[meshCount - combineMeshes.Count]);
            else if (combineMeshes.Count > meshCount)
                combineMeshes.RemoveRange(combineMeshes.Count - 1 - (combineMeshes.Count - meshCount),
                                          combineMeshes.Count - meshCount);
            var combineMeshIndex = 0;
            for (var i = 0; i < channels.Count; i++)
            {
                if (channels[i].GetMeshCount() == 0) continue;
                if (channels[i].autoCount)
                {
                    var avgBounds = 0f;
                    for (var j = 0; j < channels[i].GetMeshCount(); j++)
                        avgBounds += channels[i].GetMesh(j).bounds.size.z;
                    if (channels[i].GetMeshCount() > 1) avgBounds /= channels[i].GetMeshCount();
                    if (avgBounds > 0f)
                    {
                        var length                 = CalculateLength(channels[i].clipFrom, channels[i].clipTo);
                        var newCount               = Mathf.RoundToInt(length / avgBounds);
                        if (newCount < 1) newCount = 1;
                        channels[i].count = newCount;
                    }
                }

                channels[i].ResetIteration();
                useLastResult = false;
                var step  = 1.0  / channels[i].count;
                var space = step * channels[i].spacing * 0.5;

                switch (channels[i].type)
                {
                    case Channel.Type.Extrude:
                        for (var j = 0; j < channels[i].count; j++)
                        {
                            var from = DMath.Lerp(channels[i].clipFrom, channels[i].clipTo, j * step        + space);
                            var to   = DMath.Lerp(channels[i].clipFrom, channels[i].clipTo, j * step + step - space);
                            if (combineMeshes[combineMeshIndex] == null)
                                combineMeshes[combineMeshIndex] = new TS_Mesh();
                            Stretch(channels[i], combineMeshes[combineMeshIndex], from, to);
                            combineMeshIndex++;
                        }

                        if (space == 0f) useLastResult = true;
                        break;
                    case Channel.Type.Place:
                        for (var j = 0; j < channels[i].count; j++)
                        {
                            if (combineMeshes[combineMeshIndex] == null)
                                combineMeshes[combineMeshIndex] = new TS_Mesh();
                            Place(channels[i], combineMeshes[combineMeshIndex],
                                  DMath.Lerp(channels[i].clipFrom, channels[i].clipTo,
                                             (double) j / Mathf.Max(channels[i].count - 1, 1)));
                            combineMeshIndex++;
                        }

                        break;
                }
            }

            if (tsMesh == null) tsMesh = new TS_Mesh();
            else tsMesh.Clear();
            tsMesh.Combine(combineMeshes);
        }

        private void Place(Channel channel, TS_Mesh target, double percent)
        {
            var definition             = channel.NextMesh();
            if (target == null) target = new TS_Mesh();
            definition.Write(target, channel.overrideMaterialID ? channel.targetMaterialID : -1);
            var channeloffset = channel.NextOffset(percent);
            Evaluate(percent, evalResult);
            ModifySample(evalResult);
            var originalNormal    = evalResult.up;
            var originalRight     = evalResult.right;
            var originalDirection = evalResult.forward;
            if (channel.overrideNormal)
            {
                evalResult.forward = Vector3.Cross(evalResult.right, channel.customNormal);
                evalResult.up      = channel.customNormal;
            }

            var axisRotation = Quaternion.identity;
            var resultSize   = GetBaseSize(evalResult);
            vertexMatrix
               .SetTRS(evalResult.position + originalRight * ((offset.x + channeloffset.x) * resultSize) + originalNormal * ((offset.y + channeloffset.y) * resultSize) + originalDirection * offset.z,
                       evalResult.rotation * channel.NextPlaceRotation(percent) *
                       Quaternion.AngleAxis(rotation, Vector3.forward), channel.NextPlaceScale() * resultSize);
            normalMatrix = vertexMatrix.inverse.transpose;
            for (var i = 0; i < target.vertexCount; i++)
            {
                target.vertices[i] = vertexMatrix.MultiplyPoint3x4(definition.vertices[i]);
                target.normals[i]  = normalMatrix.MultiplyVector(definition.normals[i]);
            }

            for (var i = 0; i < Mathf.Min(target.colors.Length, definition.colors.Length); i++)
                target.colors[i] = definition.colors[i] * evalResult.color;
        }

        private void Stretch(Channel channel, TS_Mesh target, double from, double to)
        {
            var definition             = channel.NextMesh();
            if (target == null) target = new TS_Mesh();
            definition.Write(target, channel.overrideMaterialID ? channel.targetMaterialID : -1);
            var uv        = Vector2.zero;
            var trsVector = Vector3.zero;

            for (var i = 0; i < definition.vertexGroups.Count; i++)
            {
                if (useLastResult && i == definition.vertexGroups.Count) evalResult = lastResult;
                else Evaluate(DMath.Lerp(from, to, definition.vertexGroups[i].percent), evalResult);
                ModifySample(evalResult, modifiedResult);
                var originalNormal    = modifiedResult.up;
                var originalRight     = modifiedResult.right;
                var originalDirection = modifiedResult.forward;
                if (channel.overrideNormal)
                {
                    modifiedResult.forward = Vector3.Cross(modifiedResult.right, channel.customNormal);
                    modifiedResult.up      = channel.customNormal;
                }

                var channelOffset = channel.NextOffset(modifiedResult.percent);
                var channelScale  = channel.NextExtrudeScale(modifiedResult.percent);
                var scaleMod      = channel.scaleModifier.GetScale(modifiedResult);
                channelScale.x *= scaleMod.x;
                channelScale.y *= scaleMod.y;
                var channelRotation = channel.NextExtrudeRotation(modifiedResult.percent);
                var resultSize      = modifiedResult.size;
                vertexMatrix
                   .SetTRS(modifiedResult.position + originalRight * ((offset.x + channelOffset.x) * resultSize) + originalNormal * ((offset.y + channelOffset.y) * resultSize) + originalDirection * offset.z, //Position
                           modifiedResult.rotation *
                           Quaternion.AngleAxis(rotation + channelRotation, Vector3.forward), //Rotation
                           channelScale * resultSize);                                        //Scale
                normalMatrix = vertexMatrix.inverse.transpose;
                if (i == 0) lastResult.CopyFrom(evalResult);

                for (var n = 0; n < definition.vertexGroups[i].ids.Length; n++)
                {
                    var index = definition.vertexGroups[i].ids[n];
                    trsVector              = definition.vertices[index];
                    trsVector.z            = 0f;
                    target.vertices[index] = vertexMatrix.MultiplyPoint3x4(trsVector);
                    trsVector              = definition.normals[index];
                    target.normals[index]  = normalMatrix.MultiplyVector(trsVector);
                    target.colors[index]   = target.colors[index] * modifiedResult.color;
                    if (target.uv.Length > index)
                    {
                        uv = target.uv[index];
                        switch (channel.overrideUVs)
                        {
                            case Channel.UVOverride.ClampU:
                                uv.x = (float) modifiedResult.percent;
                                break;
                            case Channel.UVOverride.ClampV:
                                uv.y = (float) modifiedResult.percent;
                                break;
                            case Channel.UVOverride.UniformU:
                                uv.x = CalculateLength(0.0, ClipPercent(modifiedResult.percent));
                                break;
                            case Channel.UVOverride.UniformV:
                                uv.y = CalculateLength(0.0, ClipPercent(modifiedResult.percent));
                                break;
                        }

                        target.uv[index] = new Vector2(uv.x * uvScale.x * channel.uvScale.x,
                                                       uv.y * uvScale.y * channel.uvScale.y);
                        target.uv[index] += uvOffset + channel.uvOffset;
                    }
                }
            }
        }


        [Serializable]
        public class Channel
        {
            public delegate float FloatHandler(double percent);

            public delegate Quaternion QuaternionHandler(double percent);

            public delegate Vector2 Vector2Handler(double percent);

            public delegate Vector3 Vector3Handler(double percent);

            public enum Type
            {
                Extrude,
                Place
            }

            public enum UVOverride
            {
                None,
                ClampU,
                ClampV,
                UniformU,
                UniformV
            }

            public string name = "Channel";

            [SerializeField] [HideInInspector] private int _iterationSeed;

            [SerializeField] [HideInInspector] private int _offsetSeed;

            [SerializeField] [HideInInspector] private int _rotationSeed;

            [SerializeField] [HideInInspector] private int _scaleSeed;

            [SerializeField] internal SplineMesh owner;

            [SerializeField] [HideInInspector] private List<MeshDefinition> meshes = new List<MeshDefinition>();


            [SerializeField] [HideInInspector] private double _clipFrom;

            [SerializeField] [HideInInspector] private double _clipTo = 1.0;

            [SerializeField] [HideInInspector] private bool _randomOrder;

            [SerializeField] [HideInInspector] private UVOverride _overrideUVs = UVOverride.None;

            [SerializeField] [HideInInspector] private Vector2 _uvScale = Vector2.one;

            [SerializeField] [HideInInspector] private Vector2 _uvOffset = Vector2.zero;

            [SerializeField] [HideInInspector] private bool _overrideNormal;

            [SerializeField] [HideInInspector] private Vector3 _customNormal = Vector3.up;

            [SerializeField] [HideInInspector] private Type _type = Type.Extrude;

            [SerializeField] [HideInInspector] private int _count = 1;

            [SerializeField] [HideInInspector] private bool _autoCount;

            [SerializeField] [HideInInspector] private double _spacing;

            [SerializeField] [HideInInspector] private bool _randomRotation;

            [SerializeField] [HideInInspector] private Vector3 _minRotation = Vector3.zero;

            [SerializeField] [HideInInspector] private Vector3 _maxRotation = Vector3.zero;

            [SerializeField] [HideInInspector] private bool _randomOffset;

            [SerializeField] [HideInInspector] private Vector2 _minOffset = Vector2.one;

            [SerializeField] [HideInInspector] private Vector2 _maxOffset = Vector2.one;

            [SerializeField] [HideInInspector] private bool _randomScale;

            [SerializeField] [HideInInspector] private bool _uniformRandomScale;

            [SerializeField] [HideInInspector] private Vector3 _minScale = Vector3.one;

            [SerializeField] [HideInInspector] private Vector3 _maxScale = Vector3.one;

            [SerializeField] [HideInInspector] private bool _overrideMaterialID;

            [SerializeField] [HideInInspector] private int _targetMaterialID;

            [SerializeField] [HideInInspector] protected MeshScaleModifier _scaleModifier = new MeshScaleModifier();

            private FloatHandler      _extrudeRotationHandler;
            private Vector2Handler    _offsetHandler;
            private QuaternionHandler _placeRotationHandler;
            private Vector3Handler    _scaleHandler;

            private Random iterationRandom;
            private int    iterator;
            private Random offsetRandom;
            private Random rotationRandom;
            private Random scaleRandom;

            public Channel(string n, SplineMesh parent)
            {
                name  = n;
                owner = parent;
                Init();
            }

            public Channel(string n, Mesh inputMesh, SplineMesh parent)
            {
                name  = n;
                owner = parent;
                meshes.Add(new MeshDefinition(inputMesh));
                Init();
                Rebuild();
            }

            public double clipFrom
            {
                get => _clipFrom;
                set
                {
                    if (value != _clipFrom)
                    {
                        _clipFrom = value;
                        Rebuild();
                    }
                }
            }

            public double clipTo
            {
                get => _clipTo;
                set
                {
                    if (value != _clipTo)
                    {
                        _clipTo = value;
                        Rebuild();
                    }
                }
            }

            public bool randomOffset
            {
                get => _randomOffset;
                set
                {
                    if (value != _randomOffset)
                    {
                        _randomOffset = value;
                        Rebuild();
                    }
                }
            }

            public Vector2Handler offsetHandler
            {
                get => _offsetHandler;
                set
                {
                    if (value != _offsetHandler)
                    {
                        _offsetHandler = value;
                        Rebuild();
                    }
                }
            }

            public bool overrideMaterialID
            {
                get => _overrideMaterialID;
                set
                {
                    if (value != _overrideMaterialID)
                    {
                        _overrideMaterialID = value;
                        Rebuild();
                    }
                }
            }

            public int targetMaterialID
            {
                get => _targetMaterialID;
                set
                {
                    if (value != _targetMaterialID)
                    {
                        _targetMaterialID = value;
                        Rebuild();
                    }
                }
            }

            public bool randomRotation
            {
                get => _randomRotation;
                set
                {
                    if (value != _randomRotation)
                    {
                        _randomRotation = value;
                        Rebuild();
                    }
                }
            }

            public QuaternionHandler placeRotationHandler
            {
                get => _placeRotationHandler;
                set
                {
                    if (value != _placeRotationHandler)
                    {
                        _placeRotationHandler = value;
                        Rebuild();
                    }
                }
            }

            public FloatHandler extrudeRotationHandler
            {
                get => _extrudeRotationHandler;
                set
                {
                    if (value != _extrudeRotationHandler)
                    {
                        _extrudeRotationHandler = value;
                        Rebuild();
                    }
                }
            }

            public bool randomScale
            {
                get => _randomScale;
                set
                {
                    if (value != _randomScale)
                    {
                        _randomScale = value;
                        Rebuild();
                    }
                }
            }

            public Vector3Handler scaleHandler
            {
                get => _scaleHandler;
                set
                {
                    if (value != _scaleHandler)
                    {
                        _scaleHandler = value;
                        Rebuild();
                    }
                }
            }

            public bool uniformRandomScale
            {
                get => _uniformRandomScale;
                set
                {
                    if (value != _uniformRandomScale)
                    {
                        _uniformRandomScale = value;
                        Rebuild();
                    }
                }
            }

            public int offsetSeed
            {
                get => _offsetSeed;
                set
                {
                    if (value != _offsetSeed)
                    {
                        _offsetSeed = value;
                        Rebuild();
                    }
                }
            }

            public int rotationSeed
            {
                get => _rotationSeed;
                set
                {
                    if (value != _rotationSeed)
                    {
                        _rotationSeed = value;
                        Rebuild();
                    }
                }
            }

            public int scaleSeed
            {
                get => _scaleSeed;
                set
                {
                    if (value != _scaleSeed)
                    {
                        _scaleSeed = value;
                        Rebuild();
                    }
                }
            }

            public double spacing
            {
                get => _spacing;
                set
                {
                    if (value != _spacing)
                    {
                        _spacing = value;
                        Rebuild();
                    }
                }
            }

            public Vector2 minOffset
            {
                get => _minOffset;
                set
                {
                    if (value != _minOffset)
                    {
                        _minOffset = value;
                        Rebuild();
                    }
                }
            }

            public Vector2 maxOffset
            {
                get => _maxOffset;
                set
                {
                    if (value != _maxOffset)
                    {
                        _maxOffset = value;
                        Rebuild();
                    }
                }
            }

            public Vector3 minRotation
            {
                get => _minRotation;
                set
                {
                    if (value != _minRotation)
                    {
                        _minRotation = value;
                        Rebuild();
                    }
                }
            }

            public Vector3 maxRotation
            {
                get => _maxRotation;
                set
                {
                    if (value != _maxRotation)
                    {
                        _maxRotation = value;
                        Rebuild();
                    }
                }
            }

            public Vector3 minScale
            {
                get => _minScale;
                set
                {
                    if (value != _minScale)
                    {
                        _minScale = value;
                        Rebuild();
                    }
                }
            }

            public Vector3 maxScale
            {
                get => _maxScale;
                set
                {
                    if (value != _maxScale)
                    {
                        _maxScale = value;
                        Rebuild();
                    }
                }
            }

            public Type type
            {
                get => _type;
                set
                {
                    if (value != _type)
                    {
                        _type = value;
                        Rebuild();
                    }
                }
            }

            public bool randomOrder
            {
                get => _randomOrder;
                set
                {
                    if (value != _randomOrder)
                    {
                        _randomOrder = value;
                        Rebuild();
                    }
                }
            }

            public int randomSeed
            {
                get => _iterationSeed;
                set
                {
                    if (value != _iterationSeed)
                    {
                        _iterationSeed = value;
                        if (_randomOrder) Rebuild();
                    }
                }
            }

            public int count
            {
                get => _count;
                set
                {
                    if (value != _count)
                    {
                        _count = value;
                        if (_count < 1) _count = 1;
                        Rebuild();
                    }
                }
            }

            public bool autoCount
            {
                get => _autoCount;
                set
                {
                    if (value != _autoCount)
                    {
                        _autoCount = value;
                        Rebuild();
                    }
                }
            }

            public UVOverride overrideUVs
            {
                get => _overrideUVs;
                set
                {
                    if (value != _overrideUVs)
                    {
                        _overrideUVs = value;
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

            public bool overrideNormal
            {
                get => _overrideNormal;
                set
                {
                    if (value != _overrideNormal)
                    {
                        _overrideNormal = value;
                        Rebuild();
                    }
                }
            }

            public Vector3 customNormal
            {
                get => _customNormal;
                set
                {
                    if (value != _customNormal)
                    {
                        _customNormal = value;
                        Rebuild();
                    }
                }
            }

            public MeshScaleModifier scaleModifier => _scaleModifier;

            private void Init()
            {
                _minScale    = _maxScale    = Vector3.one;
                _minOffset   = _maxOffset   = Vector3.zero;
                _minRotation = _maxRotation = Vector3.zero;
            }

            public void CopyTo(Channel target)
            {
                target.meshes.Clear();
                for (var i = 0; i < meshes.Count; i++) target.meshes.Add(meshes[i].Copy());
                target._clipFrom               = _clipFrom;
                target._clipTo                 = _clipTo;
                target._customNormal           = _customNormal;
                target._iterationSeed          = _iterationSeed;
                target._minOffset              = _minOffset;
                target._minRotation            = _minRotation;
                target._minScale               = _minScale;
                target._maxOffset              = _maxOffset;
                target._maxRotation            = _maxRotation;
                target._maxScale               = _maxScale;
                target._randomOffset           = _randomOffset;
                target._randomRotation         = _randomRotation;
                target._randomScale            = _randomScale;
                target._offsetSeed             = _offsetSeed;
                target._offsetHandler          = _offsetHandler;
                target._rotationSeed           = _rotationSeed;
                target._placeRotationHandler   = _placeRotationHandler;
                target._extrudeRotationHandler = _extrudeRotationHandler;
                target._scaleSeed              = _scaleSeed;
                target._scaleHandler           = _scaleHandler;
                target._iterationSeed          = _iterationSeed;
                target._count                  = _count;
                target._spacing                = _spacing;
                target._overrideUVs            = _overrideUVs;
                target._type                   = _type;
                target._overrideMaterialID     = _overrideMaterialID;
                target._targetMaterialID       = _targetMaterialID;
                target._overrideNormal         = _overrideNormal;
            }

            public int GetMeshCount()
            {
                return meshes.Count;
            }

            public void SwapMeshes(int a, int b)
            {
                if (a < 0 || a >= meshes.Count || b < 0 || b >= meshes.Count) return;
                var temp = meshes[b];
                meshes[b] = meshes[a];
                meshes[a] = temp;
                Rebuild();
            }

            public void DuplicateMesh(int index)
            {
                if (index < 0 || index >= meshes.Count) return;
                meshes.Add(meshes[index].Copy());
                Rebuild();
            }

            public MeshDefinition GetMesh(int index)
            {
                return meshes[index];
            }

            public void AddMesh(Mesh input)
            {
                meshes.Add(new MeshDefinition(input));
                Rebuild();
            }

            public void RemoveMesh(int index)
            {
                meshes.RemoveAt(index);
                Rebuild();
            }

            public void ResetIteration()
            {
                if (_randomOrder) iterationRandom   = new Random(_iterationSeed);
                if (_randomOffset) offsetRandom     = new Random(_offsetSeed);
                if (_randomRotation) rotationRandom = new Random(_rotationSeed);
                if (_randomScale) scaleRandom       = new Random(_scaleSeed);
                iterator = 0;
            }

            public Vector2 NextOffset(double percent)
            {
                if (_offsetHandler != null) return _offsetHandler(percent);
                if (_randomOffset)
                    return new Vector2(Mathf.Lerp(_minOffset.x, _maxOffset.x, (float) offsetRandom.NextDouble()),
                                       Mathf.Lerp(_minOffset.y, _maxOffset.y, (float) offsetRandom.NextDouble()));
                return _minOffset;
            }

            public Quaternion NextPlaceRotation(double percent)
            {
                if (_placeRotationHandler != null) return _placeRotationHandler(percent);
                if (_randomRotation)
                    return
                        Quaternion.Euler(new
                                             Vector3(Mathf.Lerp(_minRotation.x, _maxRotation.x, (float) rotationRandom.NextDouble()),
                                                     Mathf.Lerp(_minRotation.y, _maxRotation.y,
                                                                (float) rotationRandom.NextDouble()),
                                                     Mathf.Lerp(_minRotation.z, _maxRotation.z,
                                                                (float) rotationRandom.NextDouble())));
                return Quaternion.Euler(_minRotation);
            }

            public float NextExtrudeRotation(double percent)
            {
                if (_extrudeRotationHandler != null) return _extrudeRotationHandler(percent);
                if (_randomRotation)
                    return Mathf.Lerp(_minRotation.z, _maxRotation.z, (float) rotationRandom.NextDouble());
                return _minRotation.z;
            }

            public Vector3 NextExtrudeScale(double percent)
            {
                if (_scaleHandler != null) return _scaleHandler(percent);
                if (_randomScale)
                {
                    if (_uniformRandomScale)
                        return Vector3.Lerp(new Vector3(_minScale.x, _minScale.y, 1f),
                                            new Vector3(_maxScale.x, _maxScale.y, 1f),
                                            (float) scaleRandom.NextDouble());
                    return new Vector3(Mathf.Lerp(_minScale.x, _maxScale.x, (float) scaleRandom.NextDouble()),
                                       Mathf.Lerp(_minScale.y, _maxScale.y, (float) scaleRandom.NextDouble()), 1f);
                }

                return new Vector3(_minScale.x, _minScale.y, 1f);
            }

            public Vector3 NextPlaceScale()
            {
                if (_randomScale)
                {
                    if (_uniformRandomScale)
                        return Vector3.Lerp(_minScale, _maxScale, (float) scaleRandom.NextDouble());
                    return new Vector3(Mathf.Lerp(_minScale.x, _maxScale.x, (float) scaleRandom.NextDouble()),
                                       Mathf.Lerp(_minScale.y, _maxScale.y, (float) scaleRandom.NextDouble()),
                                       Mathf.Lerp(_minScale.z, _maxScale.z, (float) scaleRandom.NextDouble()));
                }

                return _minScale;
            }

            public MeshDefinition NextMesh()
            {
                if (_randomOrder)
                {
                    return meshes[iterationRandom.Next(meshes.Count)];
                }

                if (iterator >= meshes.Count) iterator = 0;
                return meshes[iterator++];
            }

            internal void Rebuild()
            {
                if (owner != null) owner.Rebuild();
            }

            private void Refresh()
            {
                for (var i = 0; i < meshes.Count; i++) meshes[i].Refresh();
                Rebuild();
            }

            [Serializable]
            public class MeshDefinition
            {
                public enum MirrorMethod
                {
                    None,
                    X,
                    Y,
                    Z
                }

                [SerializeField] [HideInInspector] internal Vector3[] vertices = new Vector3[0];

                [SerializeField] [HideInInspector] internal Vector3[] normals = new Vector3[0];

                [SerializeField] [HideInInspector] internal Vector4[] tangents = new Vector4[0];

                [SerializeField] [HideInInspector] internal Color[] colors = new Color[0];

                [SerializeField] [HideInInspector] internal Vector2[] uv = new Vector2[0];

                [SerializeField] [HideInInspector] internal Vector2[] uv2 = new Vector2[0];

                [SerializeField] [HideInInspector] internal Vector2[] uv3 = new Vector2[0];

                [SerializeField] [HideInInspector] internal Vector2[] uv4 = new Vector2[0];

                [SerializeField] [HideInInspector] internal int[] triangles = new int[0];

                [SerializeField] [HideInInspector] internal List<Submesh> subMeshes = new List<Submesh>();

                [SerializeField] [HideInInspector]
                internal TS_Bounds bounds = new TS_Bounds(Vector3.zero, Vector3.zero);

                [SerializeField] [HideInInspector] internal List<VertexGroup> vertexGroups = new List<VertexGroup>();

                [SerializeField] [HideInInspector] private Mesh _mesh;

                [SerializeField] [HideInInspector] private Vector3 _rotation = Vector3.zero;

                [SerializeField] [HideInInspector] private Vector3 _offset = Vector3.zero;

                [SerializeField] [HideInInspector] private Vector3 _scale = Vector3.one;

                [SerializeField] [HideInInspector] private Vector2 _uvScale = Vector2.one;

                [SerializeField] [HideInInspector] private Vector2 _uvOffset = Vector2.zero;

                [SerializeField] [HideInInspector] private float _uvRotation;

                [SerializeField] [HideInInspector] private MirrorMethod _mirror = MirrorMethod.None;


                [SerializeField] [HideInInspector] private float _vertexGroupingMargin;

                [SerializeField] [HideInInspector] private bool _removeInnerFaces;

                [SerializeField] [HideInInspector] private bool _flipFaces;

                [SerializeField] [HideInInspector] private bool _doubleSided;

                public MeshDefinition(Mesh input)
                {
                    _mesh = input;
                    Refresh();
                }

                public Mesh mesh
                {
                    get => _mesh;
                    set
                    {
                        if (_mesh != value)
                        {
                            _mesh = value;
                            Refresh();
                        }
                    }
                }

                public Vector3 rotation
                {
                    get => _rotation;
                    set
                    {
                        if (rotation != value)
                        {
                            _rotation = value;
                            Refresh();
                        }
                    }
                }

                public Vector3 offset
                {
                    get => _offset;
                    set
                    {
                        if (_offset != value)
                        {
                            _offset = value;
                            Refresh();
                        }
                    }
                }

                public Vector3 scale
                {
                    get => _scale;
                    set
                    {
                        if (_scale != value)
                        {
                            _scale = value;
                            Refresh();
                        }
                    }
                }

                public Vector2 uvScale
                {
                    get => _uvScale;
                    set
                    {
                        if (_uvScale != value)
                        {
                            _uvScale = value;
                            Refresh();
                        }
                    }
                }

                public Vector2 uvOffset
                {
                    get => _uvOffset;
                    set
                    {
                        if (_uvOffset != value)
                        {
                            _uvOffset = value;
                            Refresh();
                        }
                    }
                }

                public float uvRotation
                {
                    get => _uvRotation;
                    set
                    {
                        if (_uvRotation != value)
                        {
                            _uvRotation = value;
                            Refresh();
                        }
                    }
                }

                public float vertexGroupingMargin
                {
                    get => _vertexGroupingMargin;
                    set
                    {
                        if (_vertexGroupingMargin != value)
                        {
                            _vertexGroupingMargin = value;
                            Refresh();
                        }
                    }
                }

                public MirrorMethod mirror
                {
                    get => _mirror;
                    set
                    {
                        if (_mirror != value)
                        {
                            _mirror = value;
                            Refresh();
                        }
                    }
                }

                public bool removeInnerFaces
                {
                    get => _removeInnerFaces;
                    set
                    {
                        if (_removeInnerFaces != value)
                        {
                            _removeInnerFaces = value;
                            Refresh();
                        }
                    }
                }

                public bool flipFaces
                {
                    get => _flipFaces;
                    set
                    {
                        if (_flipFaces != value)
                        {
                            _flipFaces = value;
                            Refresh();
                        }
                    }
                }

                public bool doubleSided
                {
                    get => _doubleSided;
                    set
                    {
                        if (_doubleSided != value)
                        {
                            _doubleSided = value;
                            Refresh();
                        }
                    }
                }

                internal MeshDefinition Copy()
                {
                    var target = new MeshDefinition(_mesh);
                    target.vertices  = new Vector3[vertices.Length];
                    target.normals   = new Vector3[normals.Length];
                    target.colors    = new Color[colors.Length];
                    target.tangents  = new Vector4[tangents.Length];
                    target.uv        = new Vector2[uv.Length];
                    target.uv2       = new Vector2[uv2.Length];
                    target.uv3       = new Vector2[uv3.Length];
                    target.uv4       = new Vector2[uv4.Length];
                    target.triangles = new int[triangles.Length];

                    vertices.CopyTo(target.vertices, 0);
                    normals.CopyTo(target.normals, 0);
                    colors.CopyTo(target.colors, 0);
                    tangents.CopyTo(target.tangents, 0);
                    uv.CopyTo(target.uv, 0);
                    uv2.CopyTo(target.uv2, 0);
                    uv3.CopyTo(target.uv3, 0);
                    uv4.CopyTo(target.uv4, 0);
                    triangles.CopyTo(target.triangles, 0);

                    target.bounds    = new TS_Bounds(bounds.min, bounds.max);
                    target.subMeshes = new List<Submesh>();
                    for (var i = 0; i < subMeshes.Count; i++)
                    {
                        target.subMeshes.Add(new Submesh(new int[subMeshes[i].triangles.Length]));
                        subMeshes[i].triangles.CopyTo(target.subMeshes[target.subMeshes.Count - 1].triangles, 0);
                    }

                    target._mirror      = _mirror;
                    target._offset      = _offset;
                    target._rotation    = _rotation;
                    target._scale       = _scale;
                    target._uvOffset    = _uvOffset;
                    target._uvScale     = _uvScale;
                    target._uvRotation  = _uvRotation;
                    target._flipFaces   = _flipFaces;
                    target._doubleSided = _doubleSided;
                    return target;
                }

                public void Refresh()
                {
                    if (_mesh == null)
                    {
                        vertices     = new Vector3[0];
                        normals      = new Vector3[0];
                        colors       = new Color[0];
                        uv           = new Vector2[0];
                        uv2          = new Vector2[0];
                        uv3          = new Vector2[0];
                        uv4          = new Vector2[0];
                        tangents     = new Vector4[0];
                        triangles    = new int[0];
                        subMeshes    = new List<Submesh>();
                        vertexGroups = new List<VertexGroup>();
                        return;
                    }

                    if (vertices.Length  != _mesh.vertexCount) vertices       = new Vector3[_mesh.vertexCount];
                    if (normals.Length   != _mesh.normals.Length) normals     = new Vector3[_mesh.normals.Length];
                    if (colors.Length    != _mesh.colors.Length) colors       = new Color[_mesh.colors.Length];
                    if (uv.Length        != _mesh.uv.Length) uv               = new Vector2[_mesh.uv.Length];
                    if (uv2.Length       != _mesh.uv2.Length) uv2             = new Vector2[_mesh.uv2.Length];
                    if (uv3.Length       != _mesh.uv3.Length) uv3             = new Vector2[_mesh.uv3.Length];
                    if (uv4.Length       != _mesh.uv4.Length) uv4             = new Vector2[_mesh.uv4.Length];
                    if (tangents.Length  != _mesh.tangents.Length) tangents   = new Vector4[_mesh.tangents.Length];
                    if (triangles.Length != _mesh.triangles.Length) triangles = new int[_mesh.triangles.Length];

                    vertices  = _mesh.vertices;
                    normals   = _mesh.normals;
                    colors    = _mesh.colors;
                    uv        = _mesh.uv;
                    uv2       = _mesh.uv2;
                    uv3       = _mesh.uv3;
                    uv4       = _mesh.uv4;
                    tangents  = _mesh.tangents;
                    triangles = _mesh.triangles;
                    colors    = _mesh.colors;

                    while (subMeshes.Count > _mesh.subMeshCount) subMeshes.RemoveAt(0);
                    while (subMeshes.Count < _mesh.subMeshCount) subMeshes.Add(new Submesh(new int[0]));
                    for (var i = 0; i      < subMeshes.Count; i++) subMeshes[i].triangles = _mesh.GetTriangles(i);


                    if (colors.Length != vertices.Length)
                    {
                        colors = new Color[vertices.Length];
                        for (var i = 0; i < colors.Length; i++) colors[i] = Color.white;
                    }

                    Mirror();
                    if (_doubleSided) DoubleSided();
                    else if (_flipFaces) FlipFaces();
                    TransformVertices();
                    CalculateBounds();
                    if (_removeInnerFaces) RemoveInnerFaces();
                    GroupVertices();
                }

                private void RemoveInnerFaces()
                {
                    float min = float.MaxValue, max = 0f;
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        if (vertices[i].z < min) min = vertices[i].z;
                        if (vertices[i].z > max) max = vertices[i].z;
                    }

                    for (var i = 0; i < subMeshes.Count; i++)
                    {
                        var newTris = new List<int>();
                        for (var j = 0; j < subMeshes[i].triangles.Length; j += 3)
                        {
                            bool innerMax = true, innerMin = true;
                            for (var k = j; k < j + 3; k++)
                            {
                                var index                                                  = subMeshes[i].triangles[k];
                                if (!Mathf.Approximately(vertices[index].z, max)) innerMax = false;
                                if (!Mathf.Approximately(vertices[index].z, min)) innerMin = false;
                            }

                            if (!innerMax && !innerMin)
                            {
                                newTris.Add(subMeshes[i].triangles[j]);
                                newTris.Add(subMeshes[i].triangles[j + 1]);
                                newTris.Add(subMeshes[i].triangles[j + 2]);
                            }
                        }

                        subMeshes[i].triangles = newTris.ToArray();
                    }
                }

                private void FlipFaces()
                {
                    var temp = new TS_Mesh();
                    temp.normals   = normals;
                    temp.tangents  = tangents;
                    temp.triangles = triangles;
                    for (var i = 0; i < subMeshes.Count; i++) temp.subMeshes.Add(subMeshes[i].triangles);
                    MeshUtility.FlipFaces(temp);
                }

                private void DoubleSided()
                {
                    var temp = new TS_Mesh();
                    temp.vertices  = vertices;
                    temp.normals   = normals;
                    temp.tangents  = tangents;
                    temp.colors    = colors;
                    temp.uv        = uv;
                    temp.uv2       = uv2;
                    temp.uv3       = uv3;
                    temp.uv4       = uv4;
                    temp.triangles = triangles;
                    for (var i = 0; i < subMeshes.Count; i++) temp.subMeshes.Add(subMeshes[i].triangles);
                    MeshUtility.MakeDoublesided(temp);
                    vertices  = temp.vertices;
                    normals   = temp.normals;
                    tangents  = temp.tangents;
                    colors    = temp.colors;
                    uv        = temp.uv;
                    uv2       = temp.uv2;
                    uv3       = temp.uv3;
                    uv4       = temp.uv4;
                    triangles = temp.triangles;
                    for (var i = 0; i < subMeshes.Count; i++) subMeshes[i].triangles = temp.subMeshes[i];
                }

                public void Write(TS_Mesh target, int forceMaterialId = -1)
                {
                    if (target.vertices.Length  != vertices.Length) target.vertices   = new Vector3[vertices.Length];
                    if (target.normals.Length   != normals.Length) target.normals     = new Vector3[normals.Length];
                    if (target.colors.Length    != colors.Length) target.colors       = new Color[colors.Length];
                    if (target.uv.Length        != uv.Length) target.uv               = new Vector2[uv.Length];
                    if (target.uv2.Length       != uv2.Length) target.uv2             = new Vector2[uv2.Length];
                    if (target.uv3.Length       != uv3.Length) target.uv3             = new Vector2[uv3.Length];
                    if (target.uv4.Length       != uv4.Length) target.uv4             = new Vector2[uv4.Length];
                    if (target.tangents.Length  != tangents.Length) target.tangents   = new Vector4[tangents.Length];
                    if (target.triangles.Length != triangles.Length) target.triangles = new int[triangles.Length];

                    vertices.CopyTo(target.vertices, 0);
                    normals.CopyTo(target.normals, 0);
                    colors.CopyTo(target.colors, 0);
                    uv.CopyTo(target.uv, 0);
                    uv2.CopyTo(target.uv2, 0);
                    uv3.CopyTo(target.uv3, 0);
                    uv4.CopyTo(target.uv4, 0);
                    tangents.CopyTo(target.tangents, 0);
                    triangles.CopyTo(target.triangles, 0);

                    if (target.subMeshes == null) target.subMeshes = new List<int[]>();

                    if (forceMaterialId >= 0)
                    {
                        while (target.subMeshes.Count > forceMaterialId + 1) target.subMeshes.RemoveAt(0);
                        while (target.subMeshes.Count < forceMaterialId + 1) target.subMeshes.Add(new int[0]);
                        for (var i = 0; i < target.subMeshes.Count; i++)
                            if (i != forceMaterialId)
                            {
                                if (target.subMeshes[i].Length > 0) target.subMeshes[i] = new int[0];
                            }
                            else
                            {
                                if (target.subMeshes[i].Length != triangles.Length)
                                    target.subMeshes[i] = new int[triangles.Length];
                                triangles.CopyTo(target.subMeshes[i], 0);
                            }
                    }
                    else
                    {
                        while (target.subMeshes.Count > subMeshes.Count) target.subMeshes.RemoveAt(0);
                        while (target.subMeshes.Count < subMeshes.Count) target.subMeshes.Add(new int[0]);
                        for (var i = 0; i < subMeshes.Count; i++)
                        {
                            if (subMeshes[i].triangles.Length != target.subMeshes[i].Length)
                                target.subMeshes[i] = new int[subMeshes[i].triangles.Length];
                            subMeshes[i].triangles.CopyTo(target.subMeshes[i], 0);
                        }
                    }
                }

                private void CalculateBounds()
                {
                    var min = Vector3.zero;
                    var max = Vector3.zero;
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        if (vertices[i].x      < min.x) min.x = vertices[i].x;
                        else if (vertices[i].x > max.x) max.x = vertices[i].x;
                        if (vertices[i].y      < min.y) min.y = vertices[i].y;
                        else if (vertices[i].y > max.y) max.y = vertices[i].y;
                        if (vertices[i].z      < min.z) min.z = vertices[i].z;
                        else if (vertices[i].z > max.z) max.z = vertices[i].z;
                    }

                    bounds.CreateFromMinMax(min, max);
                }

                private void Mirror()
                {
                    if (_mirror == MirrorMethod.None) return;
                    switch (_mirror)
                    {
                        case MirrorMethod.X:
                            for (var i = 0; i < vertices.Length; i++)
                            {
                                vertices[i].x *= -1f;
                                normals[i].x  =  -normals[i].x;
                            }

                            break;
                        case MirrorMethod.Y:
                            for (var i = 0; i < vertices.Length; i++)
                            {
                                vertices[i].y *= -1f;
                                normals[i].y  =  -normals[i].y;
                            }

                            break;
                        case MirrorMethod.Z:
                            for (var i = 0; i < vertices.Length; i++)
                            {
                                vertices[i].z *= -1f;
                                normals[i].z  =  -normals[i].z;
                            }

                            break;
                    }

                    for (var i = 0; i < triangles.Length; i += 3)
                    {
                        var temp = triangles[i];
                        triangles[i] = triangles[i + 2];
                        triangles[i                + 2] = temp;
                    }

                    for (var i = 0; i < subMeshes.Count; i++)
                    {
                        for (var j = 0; j < subMeshes[i].triangles.Length; j += 3)
                        {
                            var temp = subMeshes[i].triangles[j];
                            subMeshes[i].triangles[j] = subMeshes[i].triangles[j + 2];
                            subMeshes[i].triangles[j                             + 2] = temp;
                        }
                    }

                    CalculateTangents();
                }

                private void TransformVertices()
                {
                    var vertexMatrix = new Matrix4x4();
                    vertexMatrix.SetTRS(_offset, Quaternion.Euler(_rotation), _scale);
                    var normalMatrix = vertexMatrix.inverse.transpose;
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = vertexMatrix.MultiplyPoint3x4(vertices[i]);
                        normals[i]  = normalMatrix.MultiplyVector(normals[i]).normalized;
                    }

                    for (var i = 0; i < tangents.Length; i++) tangents[i] = normalMatrix.MultiplyVector(tangents[i]);
                    for (var i = 0; i < uv.Length; i++)
                    {
                        uv[i].x *= _uvScale.x;
                        uv[i].y *= _uvScale.y;
                        uv[i]   += _uvOffset;
                        uv[i]   =  Quaternion.AngleAxis(uvRotation, Vector3.forward) * uv[i];
                    }
                }

                private void GroupVertices()
                {
                    vertexGroups = new List<VertexGroup>();

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var value   = vertices[i].z;
                        var percent = DMath.Clamp01(DMath.InverseLerp(bounds.min.z, bounds.max.z, value));
                        var index   = FindInsertIndex(vertices[i], value);
                        if (index >= vertexGroups.Count)
                        {
                            vertexGroups.Add(new VertexGroup(value, percent, new[] {i}));
                        }
                        else
                        {
                            var valueDelta = Mathf.Abs(vertexGroups[index].value - value);
                            if (valueDelta < vertexGroupingMargin ||
                                Mathf.Approximately(valueDelta, vertexGroupingMargin))
                            {
                                vertexGroups[index].AddId(i);
                            }
                            else if (vertexGroups[index].value < value)
                            {
                                vertexGroups.Insert(index, new VertexGroup(value, percent, new[] {i}));
                            }
                            else
                            {
                                if (index < vertexGroups.Count - 1)
                                    vertexGroups.Insert(index + 1, new VertexGroup(value, percent, new[] {i}));
                                else vertexGroups.Add(new VertexGroup(value,              percent, new[] {i}));
                            }
                        }
                    }
                }

                private int FindInsertIndex(Vector3 pos, float value)
                {
                    var lower = 0;
                    var upper = vertexGroups.Count - 1;

                    while (lower <= upper)
                    {
                        var middle = lower + (upper - lower) / 2;
                        if (vertexGroups[middle].value == value) return middle;
                        if (vertexGroups[middle].value < value) upper = middle - 1;
                        else lower                                    = middle + 1;
                    }

                    return lower;
                }

                private void CalculateTangents()
                {
                    if (vertices.Length == 0)
                    {
                        tangents = new Vector4[0];
                        return;
                    }

                    tangents = new Vector4[vertices.Length];
                    var tan1 = new Vector3[vertices.Length];
                    var tan2 = new Vector3[vertices.Length];
                    for (var i = 0; i < subMeshes.Count; i++)
                    {
                        for (var j = 0; j < subMeshes[i].triangles.Length; j += 3)
                        {
                            var i1  = subMeshes[i].triangles[j];
                            var i2  = subMeshes[i].triangles[j + 1];
                            var i3  = subMeshes[i].triangles[j + 2];
                            var x1  = vertices[i2].x           - vertices[i1].x;
                            var x2  = vertices[i3].x           - vertices[i1].x;
                            var y1  = vertices[i2].y           - vertices[i1].y;
                            var y2  = vertices[i3].y           - vertices[i1].y;
                            var z1  = vertices[i2].z           - vertices[i1].z;
                            var z2  = vertices[i3].z           - vertices[i1].z;
                            var s1  = uv[i2].x                 - uv[i1].x;
                            var s2  = uv[i3].x                 - uv[i1].x;
                            var t1  = uv[i2].y                 - uv[i1].y;
                            var t2  = uv[i3].y                 - uv[i1].y;
                            var div = s1                  * t2 - s2 * t1;
                            var r   = div == 0f ? 0f : 1f / div;
                            var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,
                                                   (t2 * z1 - t1 * z2) * r);
                            var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r,
                                                   (s1 * z2 - s2 * z1) * r);
                            tan1[i1] += sdir;
                            tan1[i2] += sdir;
                            tan1[i3] += sdir;
                            tan2[i1] += tdir;
                            tan2[i2] += tdir;
                            tan2[i3] += tdir;
                        }
                    }

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var n = normals[i];
                        var t = tan1[i];
                        Vector3.OrthoNormalize(ref n, ref t);
                        tangents[i].x = t.x;
                        tangents[i].y = t.y;
                        tangents[i].z = t.z;
                        tangents[i].w = Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ? -1.0f : 1.0f;
                    }
                }

                [Serializable]
                public class Submesh
                {
                    public int[] triangles = new int[0];

                    public Submesh()
                    {
                    }

                    public Submesh(int[] input)
                    {
                        triangles = new int[input.Length];
                        input.CopyTo(triangles, 0);
                    }
                }

                [Serializable]
                public class VertexGroup
                {
                    public float  value;
                    public double percent;
                    public int[]  ids;

                    public VertexGroup(float val, double perc, int[] vertIds)
                    {
                        percent = perc;
                        value   = val;
                        ids     = vertIds;
                    }

                    public void AddId(int id)
                    {
                        var newIds = new int[ids.Length + 1];
                        ids.CopyTo(newIds, 0);
                        newIds[newIds.Length - 1] = id;
                        ids                       = newIds;
                    }
                }
            }
        }
    }
}