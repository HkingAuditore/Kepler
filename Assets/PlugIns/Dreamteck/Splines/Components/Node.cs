using System;
using UnityEngine;

namespace Dreamteck.Splines
{
    [ExecuteInEditMode]
    [AddComponentMenu("Dreamteck/Splines/Node Connector")]
    public class Node : MonoBehaviour
    {
        public enum Type
        {
            Smooth,
            Free
        }

        [HideInInspector] public Type type = Type.Smooth;

        [SerializeField] [HideInInspector] protected Connection[] connections = new Connection[0];

        [SerializeField] [HideInInspector] private bool _transformSize = true;

        [SerializeField] [HideInInspector] private bool _transformNormals = true;

        [SerializeField] [HideInInspector] private bool _transformTangents = true;

        private Vector3    lastPosition, lastScale;
        private Quaternion lastRotation;
        private Transform  trs;

        public bool transformNormals
        {
            get => _transformNormals;
            set
            {
                if (value != _transformNormals)
                {
                    _transformNormals = value;
                    UpdatePoints();
                }
            }
        }

        public bool transformSize
        {
            get => _transformSize;
            set
            {
                if (value != _transformSize)
                {
                    _transformSize = value;
                    UpdatePoints();
                }
            }
        }

        public bool transformTangents
        {
            get => _transformTangents;
            set
            {
                if (value != _transformTangents)
                {
                    _transformTangents = value;
                    UpdatePoints();
                }
            }
        }

        private void Awake()
        {
            trs = transform;
            SampleTransform();
        }

        private void Update()
        {
            Run();
        }


        private void LateUpdate()
        {
            Run();
        }

        private void OnDestroy()
        {
            ClearConnections();
        }

        private bool TransformChanged()
        {
#if UNITY_EDITOR
            if (trs == null)
                return lastPosition != transform.position || lastRotation != transform.rotation ||
                       lastScale    != transform.lossyScale;
#endif
            return lastPosition != trs.position || lastRotation != trs.rotation || lastScale != trs.lossyScale;
        }

        private void SampleTransform()
        {
#if UNITY_EDITOR
            lastPosition = transform.position;
            lastScale    = transform.lossyScale;
            lastRotation = transform.rotation;
#else
            lastPosition = trs.position;
            lastScale = trs.lossyScale;
            lastRotation = trs.rotation;
#endif
        }

        private void Run()
        {
            if (TransformChanged())
            {
                UpdateConnectedComputers();
                SampleTransform();
            }
        }

        public SplinePoint GetPoint(int connectionIndex, bool swapTangents)
        {
            var point = PointToWorld(connections[connectionIndex].point);
            if (connections[connectionIndex].invertTangents && swapTangents)
            {
                var tempTan = point.tangent;
                point.tangent  = point.tangent2;
                point.tangent2 = tempTan;
            }

            return point;
        }

        public void SetPoint(int connectionIndex, SplinePoint worldPoint, bool swappedTangents)
        {
            var connection = connections[connectionIndex];
            connection.point = PointToLocal(worldPoint);
            if (connection.invertTangents && swappedTangents)
            {
                var tempTan = connection.point.tangent;
                connection.point.tangent  = connection.point.tangent2;
                connection.point.tangent2 = tempTan;
            }

            if (type == Type.Smooth)
            {
                if (connection.point.type == SplinePoint.Type.SmoothFree)
                    for (var i = 0; i < connections.Length; i++)
                    {
                        if (i == connectionIndex) continue;
                        var tanDir = (connection.point.tangent - connection.point.position).normalized;
                        if (tanDir == Vector3.zero)
                            tanDir = -(connection.point.tangent2 - connection.point.position).normalized;
                        var tan1Length = (connections[i].point.tangent  - connections[i].point.position).magnitude;
                        var tan2Length = (connections[i].point.tangent2 - connections[i].point.position).magnitude;
                        connections[i].point          = connection.point;
                        connections[i].point.tangent  = connections[i].point.position + tanDir * tan1Length;
                        connections[i].point.tangent2 = connections[i].point.position - tanDir * tan2Length;
                    }
                else
                    for (var i = 0; i < connections.Length; i++)
                    {
                        if (i == connectionIndex) continue;
                        connections[i].point = connection.point;
                    }
            }
        }

        public void ClearConnections()
        {
            for (var i = connections.Length - 1; i >= 0; i--)
                if (connections[i].spline != null)
                    connections[i].spline.DisconnectNode(connections[i].pointIndex);
            connections = new Connection[0];
        }

        public void UpdateConnectedComputers(SplineComputer excludeComputer = null)
        {
            for (var i = connections.Length - 1; i >= 0; i--)
            {
                if (!connections[i].isValid)
                {
                    RemoveConnection(i);
                    continue;
                }

                if (connections[i].spline == excludeComputer) continue;
                if (type == Type.Smooth && i != 0) SetPoint(i, GetPoint(0, false), false);
                var point                           = GetPoint(i, true);
                if (!transformNormals) point.normal = connections[i].spline.GetPointNormal(connections[i].pointIndex);
                if (!transformTangents)
                {
                    point.tangent  = connections[i].spline.GetPointTangent(connections[i].pointIndex);
                    point.tangent2 = connections[i].spline.GetPointTangent2(connections[i].pointIndex);
                }

                if (!transformSize) point.size = connections[i].spline.GetPointSize(connections[i].pointIndex);
                connections[i].spline.SetPoint(connections[i].pointIndex, point);
            }
        }

        public void UpdatePoint(SplineComputer computer, int pointIndex, SplinePoint point, bool updatePosition = true)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) transform.position = point.position;
            else trs.position                              = point.position;
#else
            trs.position = point.position;
#endif
            for (var i = 0; i < connections.Length; i++)
                if (connections[i].spline == computer && connections[i].pointIndex == pointIndex)
                    SetPoint(i, point, true);
        }

        public void UpdatePoints()
        {
            for (var i = connections.Length - 1; i >= 0; i--)
            {
                if (!connections[i].isValid)
                {
                    RemoveConnection(i);
                    continue;
                }

                var point = connections[i].spline.GetPoint(connections[i].pointIndex);
                point.SetPosition(transform.position);
                SetPoint(i, point, true);
            }
        }

#if UNITY_EDITOR
        //Use this to maintain the connections between computers in the editor
        public void EditorMaintainConnections()
        {
            RemoveInvalidConnections();
        }
#endif
        //Remove invalid connections
        protected void RemoveInvalidConnections()
        {
            for (var i = connections.Length - 1; i >= 0; i--)
                if (connections[i] == null || !connections[i].isValid)
                    RemoveConnection(i);
        }

        public virtual void AddConnection(SplineComputer computer, int pointIndex)
        {
            RemoveInvalidConnections();
            var connected = computer.GetNode(pointIndex);
            if (connected != null)
            {
                Debug.LogError(computer.name + " is already connected to node " + connected.name + " at point " +
                               pointIndex);
                return;
            }

            var point = computer.GetPoint(pointIndex);
            point.SetPosition(transform.position);
            ArrayUtility.Add(ref connections, new Connection(computer, pointIndex, PointToLocal(point)));
            if (connections.Length == 1) SetPoint(connections.Length - 1, point, true);
            UpdateConnectedComputers();
        }

        protected SplinePoint PointToLocal(SplinePoint worldPoint)
        {
            worldPoint.position =  Vector3.zero;
            worldPoint.tangent  =  transform.InverseTransformPoint(worldPoint.tangent);
            worldPoint.tangent2 =  transform.InverseTransformPoint(worldPoint.tangent2);
            worldPoint.normal   =  transform.InverseTransformDirection(worldPoint.normal);
            worldPoint.size     /= (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
            return worldPoint;
        }

        protected SplinePoint PointToWorld(SplinePoint localPoint)
        {
            localPoint.position =  transform.position;
            localPoint.tangent  =  transform.TransformPoint(localPoint.tangent);
            localPoint.tangent2 =  transform.TransformPoint(localPoint.tangent2);
            localPoint.normal   =  transform.TransformDirection(localPoint.normal);
            localPoint.size     *= (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
            return localPoint;
        }

        public virtual void RemoveConnection(SplineComputer computer, int pointIndex)
        {
            var index = -1;
            for (var i = 0; i < connections.Length; i++)
                if (connections[i].pointIndex == pointIndex && connections[i].spline == computer)
                {
                    index = i;
                    break;
                }

            if (index < 0) return;
            RemoveConnection(index);
        }

        private void RemoveConnection(int index)
        {
            var newConnections = new Connection[connections.Length - 1];
            var spline         = connections[index].spline;
            var pointIndex     = connections[index].pointIndex;
            for (var i = 0; i < connections.Length; i++)
                if (i      < index) newConnections[i] = connections[i];
                else if (i == index) continue;
                else newConnections[i - 1] = connections[i];
            connections = newConnections;
        }

        public virtual bool HasConnection(SplineComputer computer, int pointIndex)
        {
            for (var i = connections.Length - 1; i >= 0; i--)
            {
                if (!connections[i].isValid)
                {
                    RemoveConnection(i);
                    continue;
                }

                if (connections[i].spline == computer && connections[i].pointIndex == pointIndex) return true;
            }

            return false;
        }

        public Connection[] GetConnections()
        {
            return connections;
        }

        [Serializable]
        public class Connection
        {
            public bool invertTangents;

            [SerializeField] private int _pointIndex;

            [SerializeField] private SplineComputer _computer;

            [SerializeField] [HideInInspector] internal SplinePoint point;

            internal Connection(SplineComputer comp, int index, SplinePoint inputPoint)
            {
                _pointIndex = index;
                _computer   = comp;
                point       = inputPoint;
            }

            public SplineComputer spline => _computer;

            public int pointIndex => _pointIndex;

            internal bool isValid
            {
                get
                {
                    if (_computer   == null) return false;
                    if (_pointIndex >= _computer.pointCount) return false;
                    return true;
                }
            }
        }
    }
}