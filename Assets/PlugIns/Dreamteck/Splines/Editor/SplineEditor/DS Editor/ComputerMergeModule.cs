using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class ComputerMergeModule : ComputerEditorModule
    {
        public enum MergeSide
        {
            Start,
            End
        }

        private SplineComputer[] availableMergeComputers = new SplineComputer[0];
        public  bool             mergeEndpoints;
        public  MergeSide        mergeSide = MergeSide.End;

        public ComputerMergeModule(SplineComputer spline) : base(spline)
        {
        }

        public override GUIContent GetIconOff()
        {
            return IconContent("Merge", "merge", "Merge Splines");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("Merge", "merge_on", "Merge Splines");
        }

        public override void LoadState()
        {
            mergeEndpoints = LoadBool("mergeEndpoints");
            mergeSide      = (MergeSide) LoadInt("mergeSide");
        }

        public override void SaveState()
        {
            SaveBool("mergeEndpoints", mergeEndpoints);
            SaveInt("mergeSide", (int) mergeSide);
        }

        public override void Select()
        {
            base.Select();
            FindAvailableComputers();
        }

        private void FindAvailableComputers()
        {
            var found     = Object.FindObjectsOfType<SplineComputer>();
            var available = new List<SplineComputer>();
            for (var i = 0; i < found.Length; i++)
                if (found[i] != spline && !found[i].isClosed && spline.pointCount >= 2)
                    available.Add(found[i]);
            availableMergeComputers = available.ToArray();
        }

        public override void DrawScene()
        {
            base.DrawScene();
            if (spline.isClosed) return;
            var editorCamera = SceneView.currentDrawingSceneView.camera;
            for (var i = 0; i < availableMergeComputers.Length; i++)
            {
                DSSplineDrawer.DrawSplineComputer(availableMergeComputers[i]);
                var startPoint = availableMergeComputers[i].GetPoint(0);
                var endPoint   = availableMergeComputers[i].GetPoint(availableMergeComputers[i].pointCount - 1);
                Handles.color = availableMergeComputers[i].editorPathColor;

                if (SplineEditorHandles.CircleButton(startPoint.position,
                                                     Quaternion.LookRotation(editorCamera.transform.position -
                                                                             startPoint.position),
                                                     HandleUtility.GetHandleSize(startPoint.position) * 0.15f, 1f,
                                                     availableMergeComputers[i].editorPathColor))
                {
                    Merge(i, MergeSide.Start);
                    break;
                }

                if (SplineEditorHandles.CircleButton(endPoint.position,
                                                     Quaternion.LookRotation(editorCamera.transform.position -
                                                                             endPoint.position),
                                                     HandleUtility.GetHandleSize(endPoint.position) * 0.15f, 1f,
                                                     availableMergeComputers[i].editorPathColor))
                {
                    Merge(i, MergeSide.End);
                    break;
                }
            }

            Handles.color = Color.white;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (spline.isClosed)
            {
                EditorGUILayout.LabelField("Closed splines cannot be merged with others.",
                                           EditorStyles.centeredGreyMiniLabel);
                return;
            }

            mergeSide      = (MergeSide) EditorGUILayout.EnumPopup("Merge:", mergeSide);
            mergeEndpoints = EditorGUILayout.Toggle("Merge Endpoints", mergeEndpoints);
        }

        private void Merge(int index, MergeSide mergingSide)
        {
            RecordUndo("Merge Splines");
            var           mergedSpline = availableMergeComputers[index];
            var           mergedPoints = mergedSpline.GetPoints();
            var           original     = spline.GetPoints();
            var           pointsList   = new List<SplinePoint>();
            SplinePoint[] points;
            if (!mergeEndpoints) points = new SplinePoint[mergedPoints.Length                   + original.Length];
            else points                 = new SplinePoint[mergedPoints.Length + original.Length - 1];

            if (mergeSide == MergeSide.End)
            {
                if (mergingSide == MergeSide.Start)
                {
                    for (var i = 0; i                      < original.Length; i++) pointsList.Add(original[i]);
                    for (var i = mergeEndpoints ? 1 : 0; i < mergedPoints.Length; i++) pointsList.Add(mergedPoints[i]);
                }
                else
                {
                    for (var i = 0; i < original.Length; i++) pointsList.Add(original[i]);
                    for (var i = 0; i < mergedPoints.Length - (mergeEndpoints ? 1 : 0); i++)
                        pointsList.Add(mergedPoints[mergedPoints.Length - 1 - i]);
                }
            }
            else
            {
                if (mergingSide == MergeSide.Start)
                {
                    for (var i = 0; i < mergedPoints.Length - (mergeEndpoints ? 1 : 0); i++)
                        pointsList.Add(mergedPoints[mergedPoints.Length - 1 - i]);
                    for (var i = 0; i < original.Length; i++) pointsList.Add(original[i]);
                }
                else
                {
                    for (var i = mergeEndpoints ? 1 : 0; i < mergedPoints.Length; i++) pointsList.Add(mergedPoints[i]);
                    for (var i = 0; i                      < original.Length; i++) pointsList.Add(original[i]);
                }
            }

            points = pointsList.ToArray();
            var mergedPercent = (double) (mergedPoints.Length - 1) / (points.Length - 1);
            var from          = 0.0;
            var to            = 1.0;
            if (mergeSide == MergeSide.End)
            {
                from = 1.0 - mergedPercent;
                to   = 1.0;
            }
            else
            {
                from = 0.0;
                to   = mergedPercent;
            }


            var mergedNodes   = new List<Node>();
            var mergedIndices = new List<int>();

            for (var i = 0; i < mergedSpline.pointCount; i++)
            {
                var node = mergedSpline.GetNode(i);
                if (node != null)
                {
                    mergedNodes.Add(node);
                    mergedIndices.Add(i);
                    Undo.RecordObject(node, "Disconnect Node");
                    mergedSpline.DisconnectNode(i);
                    i--;
                }
            }

            var subs = mergedSpline.GetSubscribers();
            for (var i = 0; i < subs.Length; i++)
            {
                mergedSpline.Unsubscribe(subs[i]);
                subs[i].spline   = spline;
                subs[i].clipFrom = DMath.Lerp(from, to, subs[i].clipFrom);
                subs[i].clipTo   = DMath.Lerp(from, to, subs[i].clipTo);
            }

            spline.SetPoints(points);

            if (mergeSide == MergeSide.Start)
            {
                spline.ShiftNodes(0, spline.pointCount - 1, mergedSpline.pointCount);
                for (var i = 0; i < mergedNodes.Count; i++) spline.ConnectNode(mergedNodes[i], mergedIndices[i]);
            }
            else
            {
                for (var i = 0; i < mergedNodes.Count; i++)
                {
                    var connectIndex = mergedIndices[i] + original.Length;
                    if (mergeEndpoints) connectIndex--;
                    spline.ConnectNode(mergedNodes[i], connectIndex);
                }
            }

            if (EditorUtility.DisplayDialog("Keep merged computer's GameObject?",
                                            "Do you want to keep the merged computer's game object?", "Yes", "No"))
            {
                Undo.DestroyObjectImmediate(mergedSpline);
            }
            else
            {
                for (var i = 0; i < mergedNodes.Count; i++)
                    if (TransformUtility.IsParent(mergedNodes[i].transform, mergedSpline.transform))
                        Undo.SetTransformParent(mergedNodes[i].transform, mergedSpline.transform.parent,
                                                "Reparent Node");
                Undo.DestroyObjectImmediate(mergedSpline.gameObject);
            }

            FindAvailableComputers();
        }
    }
}