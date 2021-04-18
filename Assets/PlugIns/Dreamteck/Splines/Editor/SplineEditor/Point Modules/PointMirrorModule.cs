using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class PointMirrorModule : PointTransformModule
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public  Axis    axis = Axis.X;
        public  bool    flip;
        private Vector3 mirrorCenter = Vector3.zero;


        private SplinePoint[] mirrored = new SplinePoint[0];
        public  float         weldDistance;


        public PointMirrorModule(SplineEditor editor) : base(editor)
        {
            LoadState();
        }

        public override GUIContent GetIconOff()
        {
            return IconContent("||", "mirror", "Mirror Path");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("||", "mirror_on", "Mirror Path");
        }

        public override void LoadState()
        {
            axis         = (Axis) LoadInt("axis");
            flip         = LoadBool("flip");
            weldDistance = LoadFloat("weldDistance");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveInt("axis", (int) axis);
            SaveBool("flip", flip);
            SaveFloat("weldDistance", weldDistance);
        }

        public override void Select()
        {
            base.Select();
            RecordUndo("Mirror Points");
            ClearSelection();
            DoMirror();
        }

        public override void Deselect()
        {
            base.Deselect();
            if (!IsDirty()) return;
            if (EditorUtility.DisplayDialog("Unapplied Mirror Operation",
                                            "There is an unapplied mirror operation. Do you want to apply the changes?",
                                            "Apply", "Revert")) Apply();
            else Revert();
        }

        public override void DrawInspector()
        {
            if (selectedPoints.Count > 0) ClearSelection();
            EditorGUI.BeginChangeCheck();
            axis         = (Axis) EditorGUILayout.EnumPopup("Axis", axis);
            flip         = EditorGUILayout.Toggle("Flip", flip);
            weldDistance = EditorGUILayout.FloatField("Weld Distance", weldDistance);
            mirrorCenter = EditorGUILayout.Vector3Field("Center", mirrorCenter);
            if (EditorGUI.EndChangeCheck()) DoMirror();
            if (IsDirty())
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply")) Apply();
                if (GUILayout.Button("Revert")) Revert();
                EditorGUILayout.EndHorizontal();
            }
        }

        public override void DrawScene()
        {
            if (selectedPoints.Count > 0) ClearSelection();
            var worldCenter = TransformPosition(mirrorCenter);
            var lastCenter  = worldCenter;
            worldCenter  = Handles.PositionHandle(worldCenter, rotation);
            mirrorCenter = InverseTransformPosition(worldCenter);
            DrawMirror();
            if (lastCenter != worldCenter) DoMirror();
            selectedPoints.Clear();
        }

        public void DoMirror()
        {
            var half   = GetHalf(ref originalPoints);
            var welded = -1;
            if (half.Count > 0)
            {
                if (flip)
                {
                    if (IsWeldable(originalPoints[half[0]]))
                    {
                        welded = half[0];
                        half.RemoveAt(0);
                    }
                }
                else
                {
                    if (IsWeldable(originalPoints[half[half.Count - 1]]))
                    {
                        welded = half[half.Count - 1];
                        half.RemoveAt(half.Count - 1);
                    }
                }

                var offset         = welded >= 0 ? 1 : 0;
                var additionalSlot = isClosed && half.Count > 0 ? 1 : 0;
                if (additionalSlot > 0)
                {
                    if (flip)
                    {
                        if (IsWeldable(originalPoints[half[half.Count - 1]])) additionalSlot = 0;
                    }
                    else
                    {
                        if (IsWeldable(originalPoints[half[0]])) additionalSlot = 0;
                    }
                }

                var mirroredLength                              = half.Count * 2 + offset + additionalSlot;
                if (mirrored.Length != mirroredLength) mirrored = new SplinePoint[mirroredLength];
                for (var i = 0; i < half.Count; i++)
                    if (flip)
                    {
                        mirrored[i]                       = new SplinePoint(originalPoints[half[half.Count - 1 - i]]);
                        mirrored[i + half.Count + offset] = GetMirrored(originalPoints[half[i]]);
                        SwapTangents(ref mirrored[i]);
                        SwapTangents(ref mirrored[i + half.Count + offset]);
                    }
                    else
                    {
                        mirrored[i]                       = new SplinePoint(originalPoints[half[i]]);
                        mirrored[i + half.Count + offset] = GetMirrored(originalPoints[half[half.Count - 1 - i]]);
                    }

                if (welded >= 0)
                {
                    mirrored[half.Count] = new SplinePoint(originalPoints[welded]);
                    if (flip) SwapTangents(ref mirrored[half.Count]);
                    MakeMiddlePoint(ref mirrored[half.Count]);
                }

                if (isClosed && mirrored.Length > 0)
                {
                    if (additionalSlot == 0) MakeMiddlePoint(ref mirrored[0]);
                    mirrored[mirrored.Length - 1] = new SplinePoint(mirrored[0]);
                }
            }
            else
            {
                mirrored = new SplinePoint[0];
            }

            points = mirrored;
            SetDirty();
        }

        private void SwapTangents(ref SplinePoint point)
        {
            var temp = point.tangent;
            point.tangent  = point.tangent2;
            point.tangent2 = temp;
        }

        private void MakeMiddlePoint(ref SplinePoint point)
        {
            point.type = SplinePoint.Type.Broken;
            InverseTransformPoint(ref point);
            var newPos = point.position;
            switch (axis)
            {
                case Axis.X:

                    newPos.x = mirrorCenter.x;
                    point.SetPosition(newPos);
                    if (point.tangent.x >= mirrorCenter.x && flip || point.tangent.x <= mirrorCenter.x && !flip)
                    {
                        point.tangent2   = point.tangent;
                        point.tangent2.x = point.position.x + (point.position.x - point.tangent.x);
                    }
                    else
                    {
                        point.tangent   = point.tangent2;
                        point.tangent.x = point.position.x + (point.position.x - point.tangent2.x);
                    }

                    break;
                case Axis.Y:
                    newPos.y = mirrorCenter.y;
                    point.SetPosition(newPos);
                    if (point.tangent.y >= mirrorCenter.y && flip || point.tangent.y <= mirrorCenter.y && !flip)
                    {
                        point.tangent2   = point.tangent;
                        point.tangent2.y = point.position.y + (point.position.y - point.tangent.y);
                    }
                    else
                    {
                        point.tangent   = point.tangent2;
                        point.tangent.y = point.position.y + (point.position.y - point.tangent2.y);
                    }

                    break;
                case Axis.Z:
                    newPos.z = mirrorCenter.z;
                    point.SetPosition(newPos);
                    if (point.tangent.z >= mirrorCenter.z && flip || point.tangent.z <= mirrorCenter.z && !flip)
                    {
                        point.tangent2   = point.tangent;
                        point.tangent2.z = point.position.z + (point.position.z - point.tangent.z);
                    }
                    else
                    {
                        point.tangent   = point.tangent2;
                        point.tangent.z = point.position.z + (point.position.z - point.tangent2.z);
                    }

                    break;
            }

            TransformPoint(ref point);
        }

        private bool IsWeldable(SplinePoint point)
        {
            switch (axis)
            {
                case Axis.X:
                    if (Mathf.Abs(point.position.x - mirrorCenter.x) <= weldDistance) return true;
                    break;
                case Axis.Y:
                    if (Mathf.Abs(point.position.y - mirrorCenter.y) <= weldDistance) return true;
                    break;
                case Axis.Z:
                    if (Mathf.Abs(point.position.z - mirrorCenter.z) <= weldDistance) return true;
                    break;
            }

            return false;
        }

        private void DrawMirror()
        {
            var points      = new Vector3[4];
            var color       = Color.white;
            var worldCenter = TransformPosition(mirrorCenter);
            var size        = HandleUtility.GetHandleSize(worldCenter);
            var forward     = rotation * Vector3.forward * size;
            var back        = -forward;
            var right       = rotation * Vector3.right * size;
            var left        = -right;
            var up          = rotation * Vector3.up * size;
            var down        = -up;
            switch (axis)
            {
                case Axis.X:
                    points[0] = back    + up;
                    points[1] = forward + up;
                    points[2] = forward + down;
                    points[3] = back    + down;
                    color     = Color.red;
                    break;
                case Axis.Y:
                    points[0] = back    + left;
                    points[1] = forward + left;
                    points[2] = forward + right;
                    points[3] = back    + right;
                    color     = Color.green;
                    break;
                case Axis.Z:
                    points[0] = left  + up;
                    points[1] = right + up;
                    points[2] = right + down;
                    points[3] = left  + down;
                    color     = Color.blue;
                    break;
            }

            Handles.color = color;
            Handles.DrawLine(worldCenter + points[0], worldCenter + points[1]);
            Handles.DrawLine(worldCenter + points[1], worldCenter + points[2]);
            Handles.DrawLine(worldCenter + points[2], worldCenter + points[3]);
            Handles.DrawLine(worldCenter + points[3], worldCenter + points[0]);
            Handles.color = Color.white;
        }

        private SplinePoint GetMirrored(SplinePoint source)
        {
            var newPoint = new SplinePoint(source);
            InverseTransformPoint(ref newPoint);
            switch (axis)
            {
                case Axis.X:
                    newPoint.position.x =  mirrorCenter.x - (newPoint.position.x - mirrorCenter.x);
                    newPoint.normal.x   *= -1f;
                    newPoint.tangent.x  =  mirrorCenter.x - (newPoint.tangent.x  - mirrorCenter.x);
                    newPoint.tangent2.x =  mirrorCenter.x - (newPoint.tangent2.x - mirrorCenter.x);
                    break;
                case Axis.Y:
                    newPoint.position.y =  mirrorCenter.y - (newPoint.position.y - mirrorCenter.y);
                    newPoint.normal.y   *= -1f;
                    newPoint.tangent.y  =  mirrorCenter.y - (newPoint.tangent.y  - mirrorCenter.y);
                    newPoint.tangent2.y =  mirrorCenter.y - (newPoint.tangent2.y - mirrorCenter.y);
                    break;
                case Axis.Z:
                    newPoint.position.z =  mirrorCenter.z - (newPoint.position.z - mirrorCenter.z);
                    newPoint.normal.z   *= -1f;
                    newPoint.tangent.z  =  mirrorCenter.z - (newPoint.tangent.z  - mirrorCenter.z);
                    newPoint.tangent2.z =  mirrorCenter.z - (newPoint.tangent2.z - mirrorCenter.z);
                    break;
            }

            SwapTangents(ref newPoint);
            TransformPoint(ref newPoint);
            return newPoint;
        }


        private List<int> GetHalf(ref SplinePoint[] points)
        {
            var found = new List<int>();
            switch (axis)
            {
                case Axis.X:

                    for (var i = 0; i < points.Length; i++)
                    {
                        if (isClosed && i == points.Length - 1) break;
                        if (flip)
                        {
                            if (InverseTransformPosition(points[i].position).x >= mirrorCenter.x) found.Add(i);
                        }
                        else
                        {
                            if (InverseTransformPosition(points[i].position).x <= mirrorCenter.x) found.Add(i);
                        }
                    }

                    break;

                case Axis.Y:
                    for (var i = 0; i < points.Length; i++)
                    {
                        if (isClosed && i == points.Length - 1) break;
                        if (flip)
                        {
                            if (InverseTransformPosition(points[i].position).y >= mirrorCenter.y)
                            {
                                found.Add(i);
                            }
                            else
                            {
                                if (InverseTransformPosition(points[i].position).y <= mirrorCenter.y) found.Add(i);
                            }
                        }
                    }

                    break;
                case Axis.Z:
                    for (var i = 0; i < points.Length; i++)
                    {
                        if (isClosed && i == points.Length - 1) break;
                        if (flip)
                        {
                            if (InverseTransformPosition(points[i].position).z >= mirrorCenter.z) found.Add(i);
                        }
                        else
                        {
                            if (InverseTransformPosition(points[i].position).z <= mirrorCenter.z) found.Add(i);
                        }
                    }

                    break;
            }

            return found;
        }
    }
}