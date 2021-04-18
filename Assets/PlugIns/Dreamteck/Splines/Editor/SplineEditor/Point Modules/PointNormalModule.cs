using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class PointNormalModule : PointModule
    {
        public enum NormalMode
        {
            Auto,
            Free
        }

        private readonly SplineSample evalResult = new SplineSample();
        public           NormalMode   normalMode = NormalMode.Auto;

        public PointNormalModule(SplineEditor editor) : base(editor)
        {
        }

        public override GUIContent GetIconOff()
        {
            return IconContent("N", "normal", "Set Point Normals");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("N", "normal_on", "Set Point Normals");
        }

        private void SetNormals(int mode)
        {
            mode--;
            var avg                                                   = Vector3.zero;
            for (var i = 0; i        < selectedPoints.Count; i++) avg += points[selectedPoints[i]].position;
            if (selectedPoints.Count > 1) avg                         /= selectedPoints.Count;
            var editorCamera                                          = SceneView.lastActiveSceneView.camera;

            for (var i = 0; i < selectedPoints.Count; i++)
                switch (mode)
                {
                    case 0:
                        points[selectedPoints[i]].normal *= -1;
                        break;
                    case 1:
                        points[selectedPoints[i]].normal =
                            Vector3.Normalize(editorCamera.transform.position - points[selectedPoints[i]].position);
                        break;
                    case 2:
                        points[selectedPoints[i]].normal = editorCamera.transform.forward;
                        break;
                    case 3:
                        points[selectedPoints[i]].normal = CalculatePointNormal(points, selectedPoints[i], isClosed);
                        break;
                    case 4:
                        points[selectedPoints[i]].normal = Vector3.left;
                        break;
                    case 5:
                        points[selectedPoints[i]].normal = Vector3.right;
                        break;
                    case 6:
                        points[selectedPoints[i]].normal = Vector3.up;
                        break;
                    case 7:
                        points[selectedPoints[i]].normal = Vector3.down;
                        break;
                    case 8:
                        points[selectedPoints[i]].normal = Vector3.forward;
                        break;
                    case 9:
                        points[selectedPoints[i]].normal = Vector3.back;
                        break;
                    case 10:
                        points[selectedPoints[i]].normal = Vector3.Normalize(avg - points[selectedPoints[i]].position);
                        break;
                    case 11:
                        var result = new SplineSample();
                        editor.evaluateAtPoint(selectedPoints[i], result);
                        points[selectedPoints[i]].normal = Vector3.Cross(result.forward, result.right).normalized;
                        break;
                }
        }

        public static Vector3 CalculatePointNormal(SplinePoint[] points, int index, bool isClosed)
        {
            if (points.Length < 3)
            {
                Debug.Log("Spline needs to have at least 3 control points in order to calculate normals");
                return Vector3.zero;
            }

            var side1 = Vector3.zero;
            var side2 = Vector3.zero;
            if (index == 0)
            {
                if (isClosed)
                {
                    side1 = points[index].position - points[index         + 1].position;
                    side2 = points[index].position - points[points.Length - 2].position;
                }
                else
                {
                    side1 = points[0].position - points[1].position;
                    side2 = points[0].position - points[2].position;
                }
            }
            else if (index == points.Length - 1)
            {
                side1 = points[points.Length - 1].position - points[points.Length - 3].position;
                side2 = points[points.Length - 1].position - points[points.Length - 2].position;
            }
            else
            {
                side1 = points[index].position - points[index + 1].position;
                side2 = points[index].position - points[index - 1].position;
            }

            return Vector3.Cross(side1.normalized, side2.normalized).normalized;
        }

        public override void DrawInspector()
        {
            if (editor.is2D)
            {
                EditorGUILayout.LabelField("Normal editing unavailable in 2D Mode", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            normalMode = (NormalMode) EditorGUILayout.EnumPopup("Normal Mode", normalMode);

            var setNormals =
                EditorGUILayout.Popup(0,
                                      new[]
                                      {
                                          "Normal Operations", "Flip", "Look At Camera", "Align with Camera",
                                          "Calculate", "Left", "Right", "Up", "Down", "Forward", "Back",
                                          "Look At Avg. Center", "Perpendicular to Spline"
                                      });
            if (setNormals > 0) SetNormals(setNormals);
        }

        public override void DrawScene()
        {
            if (editor.is2D) return;
            for (var i = 0; i < selectedPoints.Count; i++)
            {
                if (isClosed && selectedPoints[i] == points.Length - 1) continue;
                if (normalMode == NormalMode.Free) FreeNormal(selectedPoints[i]);
                else AutoNormal(selectedPoints[i]);
            }
        }

        private void AutoNormal(int index)
        {
            editor.evaluateAtPoint(index, evalResult);
            Handles.color = highlightColor;
            Handles.DrawWireDisc(evalResult.position, evalResult.forward,
                                 HandleUtility.GetHandleSize(points[index].position) * 0.5f);
            Handles.color = color;
            var matrix = Matrix4x4.TRS(points[index].position, evalResult.rotation, Vector3.one);
            var pos = points[index].position +
                      points[index].normal * HandleUtility.GetHandleSize(points[index].position) * 0.5f;
            Handles.DrawLine(evalResult.position, pos);
            var lastPos      = pos;
            var lastLocalPos = matrix.inverse.MultiplyPoint(pos);
            pos = Handles.FreeMoveHandle(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * 0.1f,
                                         Vector3.zero, Handles.CircleHandleCap);
            if (pos != lastPos)
            {
                RecordUndo("Edit Point Normals");
                pos = matrix.inverse.MultiplyPoint(pos);
                var delta = pos - lastLocalPos;
                for (var n = 0; n < selectedPoints.Count; n++)
                {
                    if (selectedPoints[n] == index) continue;
                    editor.evaluateAtPoint(selectedPoints[n], evalResult);
                    var localMatrix =
                        Matrix4x4.TRS(points[selectedPoints[n]].position, evalResult.rotation, Vector3.one);
                    var localPos =
                        localMatrix.inverse.MultiplyPoint(points[selectedPoints[n]].position +
                                                          points[selectedPoints[n]].normal *
                                                          HandleUtility.GetHandleSize(points[selectedPoints[n]]
                                                                                         .position) * 0.5f);
                    localPos   += delta;
                    localPos.z =  0f;
                    points[selectedPoints[n]].normal =
                        (localMatrix.MultiplyPoint(localPos) - points[selectedPoints[n]].position).normalized;
                }

                pos.z                = 0f;
                pos                  = matrix.MultiplyPoint(pos);
                points[index].normal = (pos - points[index].position).normalized;
            }
        }

        private void FreeNormal(int index)
        {
            Handles.color = highlightColor;
            Handles.DrawWireDisc(points[index].position, points[index].normal,
                                 HandleUtility.GetHandleSize(points[index].position) * 0.25f);
            Handles.DrawWireDisc(points[index].position, points[index].normal,
                                 HandleUtility.GetHandleSize(points[index].position) * 0.5f);
            Handles.color = color;
            Handles.DrawLine(points[index].position,
                             points[index].position + HandleUtility.GetHandleSize(points[index].position) *
                             points[index].normal);
            var normalPos = points[index].position +
                            points[index].normal * HandleUtility.GetHandleSize(points[index].position);
            var lastNormal = points[index].normal;
            normalPos =  SplineEditorHandles.FreeMoveCircle(normalPos, HandleUtility.GetHandleSize(normalPos) * 0.1f);
            normalPos -= points[index].position;
            normalPos.Normalize();
            if (normalPos == Vector3.zero) normalPos = Vector3.up;
            if (lastNormal != normalPos)
            {
                RecordUndo("Edit Point Normals");
                points[index].normal = normalPos;
                var delta = Quaternion.FromToRotation(lastNormal, normalPos);
                for (var n = 0; n < selectedPoints.Count; n++)
                {
                    if (selectedPoints[n] == index) continue;
                    points[selectedPoints[n]].normal = delta * points[selectedPoints[n]].normal;
                }
            }
        }
    }
}