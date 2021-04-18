using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class ComputerSplitModule : ComputerEditorModule
    {
        public ComputerSplitModule(SplineComputer spline) : base(spline)
        {
        }

        public override GUIContent GetIconOff()
        {
            return IconContent("Split", "split", "Split Spline");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("Split", "split_on", "Split Spline");
        }

        public override void DrawScene()
        {
            var change       = false;
            var editorCamera = SceneView.currentDrawingSceneView.camera;

            for (var i = 0; i < spline.pointCount; i++)
            {
                var pos = spline.GetPointPosition(i);
                if (SplineEditorHandles.CircleButton(pos,
                                                     Quaternion.LookRotation(editorCamera.transform.position - pos),
                                                     HandleUtility.GetHandleSize(pos) * 0.12f, 1f,
                                                     spline.editorPathColor))
                {
                    SplitAtPoint(i);
                    change = true;
                    break;
                }
            }

            var projected = spline.Evaluate(ProjectMouse());
            if (!change)
            {
                var pointValue = (float) projected.percent * (spline.pointCount - 1);
                var pointIndex = Mathf.FloorToInt(pointValue);
                var size       = HandleUtility.GetHandleSize(projected.position) * 0.3f;
                var up = Vector3.Cross(editorCamera.transform.forward, projected.forward).normalized * size +
                         projected.position;
                var down = Vector3.Cross(projected.forward, editorCamera.transform.forward).normalized * size +
                           projected.position;
                Handles.color = spline.editorPathColor;
                Handles.DrawLine(up, down);
                Handles.color = Color.white;
                if (pointValue - pointIndex > spline.moveStep)
                    if (SplineEditorHandles.CircleButton(projected.position,
                                                         Quaternion.LookRotation(editorCamera.transform.position -
                                                                                 projected.position),
                                                         HandleUtility.GetHandleSize(projected.position) * 0.12f, 1f,
                                                         spline.editorPathColor))
                    {
                        SplitAtPercent(projected.percent);
                        change = true;
                    }

                SceneView.RepaintAll();
            }

            Handles.color = Color.white;
            DSSplineDrawer.DrawSplineComputer(spline, 0.0,               projected.percent);
            DSSplineDrawer.DrawSplineComputer(spline, projected.percent, 1.0, 0.4f);
        }


        private void HandleNodes(SplineComputer newSpline, int splitIndex)
        {
            var nodes   = new List<Node>();
            var indices = new List<int>();

            for (var i = splitIndex; i < spline.pointCount; i++)
            {
                var node = spline.GetNode(i);
                if (node != null)
                {
                    nodes.Add(node);
                    indices.Add(i);
                    spline.DisconnectNode(i);
                    i--;
                }
            }

            for (var i = 0; i < nodes.Count; i++) newSpline.ConnectNode(nodes[i], indices[i] - splitIndex);
        }

        private void SplitAtPercent(double percent)
        {
            RecordUndo("Split Spline");
            var pointValue     = (spline.pointCount - 1) * (float) percent;
            var lastPointIndex = Mathf.FloorToInt(pointValue);
            var nextPointIndex = Mathf.CeilToInt(pointValue);
            var splitPoints    = new SplinePoint[spline.pointCount - lastPointIndex];
            var lerpPercent    = Mathf.InverseLerp(lastPointIndex, nextPointIndex, pointValue);
            var splitPoint = SplinePoint.Lerp(spline.GetPoint(lastPointIndex), spline.GetPoint(nextPointIndex),
                                              lerpPercent);
            splitPoint.SetPosition(spline.EvaluatePosition(percent));
            splitPoints[0] = splitPoint;
            for (var i = 1; i < splitPoints.Length; i++) splitPoints[i] = spline.GetPoint(lastPointIndex + i);
            var newSpline                                               = CreateNewSpline();
            newSpline.SetPoints(splitPoints);

            HandleNodes(newSpline, lastPointIndex);

            var users = newSpline.GetSubscribers();
            for (var i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp(percent, 1.0, users[i].clipFrom);
                users[i].clipTo   = DMath.InverseLerp(percent, 1.0, users[i].clipTo);
            }

            splitPoints = new SplinePoint[lastPointIndex + 2];
            for (var i = 0; i <= lastPointIndex; i++) splitPoints[i] = spline.GetPoint(i);
            splitPoints[splitPoints.Length - 1] = splitPoint;
            spline.SetPoints(splitPoints);
            users = spline.GetSubscribers();
            for (var i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp(0.0, percent, users[i].clipFrom);
                users[i].clipTo   = DMath.InverseLerp(0.0, percent, users[i].clipTo);
            }
        }

        private void SplitAtPoint(int index)
        {
            RecordUndo("Split Spline");
            var splitPoints                                             = new SplinePoint[spline.pointCount - index];
            for (var i = 0; i < splitPoints.Length; i++) splitPoints[i] = spline.GetPoint(index + i);
            var newSpline                                               = CreateNewSpline();
            newSpline.SetPoints(splitPoints);

            HandleNodes(newSpline, index);

            var users = newSpline.GetSubscribers();
            for (var i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp((double) index / (spline.pointCount - 1), 1.0, users[i].clipFrom);
                users[i].clipTo   = DMath.InverseLerp((double) index / (spline.pointCount - 1), 1.0, users[i].clipTo);
            }

            splitPoints = new SplinePoint[index + 1];
            for (var i = 0; i <= index; i++) splitPoints[i] = spline.GetPoint(i);
            spline.SetPoints(splitPoints);
            users = spline.GetSubscribers();
            for (var i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp(0.0, (double) index / (spline.pointCount - 1), users[i].clipFrom);
                users[i].clipTo   = DMath.InverseLerp(0.0, (double) index / (spline.pointCount - 1), users[i].clipTo);
            }
        }

        private SplineComputer CreateNewSpline()
        {
            var go = Object.Instantiate(spline.gameObject);
            Undo.RegisterCreatedObjectUndo(go, "New Spline");
            go.name = spline.name + "_split";
            var users     = go.GetComponents<SplineUser>();
            var newSpline = go.GetComponent<SplineComputer>();
            for (var i = 0; i < users.Length; i++)
            {
                spline.Unsubscribe(users[i]);
                users[i].spline = newSpline;
                newSpline.Subscribe(users[i]);
            }

            for (var i = go.transform.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(go.transform.GetChild(i).gameObject);
            return newSpline;
        }

        private double ProjectMouse()
        {
            if (spline.pointCount == 0) return 0.0;
            var closestDistance =
                (Event.current.mousePosition - HandleUtility.WorldToGUIPoint(spline.GetPointPosition(0))).sqrMagnitude;
            var closestPercent                         = 0.0;
            var add                                    = spline.moveStep;
            if (spline.type == Spline.Type.Linear) add /= 2.0;
            var count                                  = 0;
            for (var i = add; i < 1.0; i += add)
            {
                var result = spline.Evaluate(i);
                var point  = HandleUtility.WorldToGUIPoint(result.position);
                var dist   = (point - Event.current.mousePosition).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent  = i;
                }

                count++;
            }

            return closestPercent;
        }
    }
}