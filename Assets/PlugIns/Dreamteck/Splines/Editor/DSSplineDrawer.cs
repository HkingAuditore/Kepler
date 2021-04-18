using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamteck.Splines.Editor
{
    [InitializeOnLoad]
    public static class DSSplineDrawer
    {
        private static          bool                 refreshComputers;
        private static readonly List<SplineComputer> drawComputers = new List<SplineComputer>();
        private static          Vector3[]            positions     = new Vector3[0];
        private static          Scene                currentScene;

        static DSSplineDrawer()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += AutoDrawComputers;
#else
            SceneView.onSceneGUIDelegate += AutoDrawComputers;
#endif

            FindComputers();
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += HerarchyWindowChanged;
#else
            EditorApplication.hierarchyWindowChanged += HerarchyWindowChanged;
#endif
            EditorApplication.playModeStateChanged += ModeChanged;
        }


        private static void ModeChanged(PlayModeStateChange stateChange)
        {
            refreshComputers = true;
        }

        private static void HerarchyWindowChanged()
        {
            if (currentScene != SceneManager.GetActiveScene())
            {
                currentScene = SceneManager.GetActiveScene();
                FindComputers();
            }
        }

        private static void FindComputers()
        {
            drawComputers.Clear();
            var computers = Object.FindObjectsOfType<SplineComputer>();
            drawComputers.AddRange(computers);
        }

        private static void AutoDrawComputers(SceneView current)
        {
            if (refreshComputers)
            {
                refreshComputers = false;
                FindComputers();
            }

            for (var i = 0; i < drawComputers.Count; i++)
            {
                if (!drawComputers[i].alwaysDraw)
                {
                    drawComputers.RemoveAt(i);
                    i--;
                    continue;
                }

                DrawSplineComputer(drawComputers[i]);
            }
        }

        public static void RegisterComputer(SplineComputer comp)
        {
            if (drawComputers.Contains(comp)) return;
            comp.alwaysDraw = true;
            drawComputers.Add(comp);
        }

        public static void UnregisterComputer(SplineComputer comp)
        {
            for (var i = 0; i < drawComputers.Count; i++)
                if (drawComputers[i] == comp)
                {
                    drawComputers[i].alwaysDraw = false;
                    drawComputers.RemoveAt(i);
                    return;
                }
        }

        public static void DrawSplineComputer(SplineComputer comp, double fromPercent = 0.0, double toPercent = 1.0,
                                              float          alpha = 1f)
        {
            if (comp == null) return;
            var prevColor   = Handles.color;
            var handleColor = comp.editorPathColor;
            handleColor.a = alpha;
            Handles.color = handleColor;
            if (comp.pointCount < 2) return;

            if (comp.type == Spline.Type.BSpline && comp.pointCount > 1)
            {
                var compPoints = comp.GetPoints();
                Handles.color = new Color(handleColor.r, handleColor.g, handleColor.b, 0.5f * alpha);
                for (var i = 0; i < compPoints.Length - 1; i++)
                    Handles.DrawLine(compPoints[i].position, compPoints[i + 1].position);
                Handles.color = handleColor;
            }

            if (!comp.drawThinckness)
            {
                if (positions.Length != comp.sampleCount * 2) positions = new Vector3[comp.sampleCount * 2];
                var prevPoint                                           = comp.EvaluatePosition(fromPercent);
                var pointIndex                                          = 0;
                for (var i = 1; i < comp.sampleCount; i++)
                {
                    positions[pointIndex] = prevPoint;
                    pointIndex++;
                    positions[pointIndex] = comp.samples[i].position;
                    pointIndex++;
                    prevPoint = positions[pointIndex - 1];
                }

                Handles.DrawLines(positions);
            }
            else
            {
                var editorCamera = SceneView.currentDrawingSceneView.camera.transform;
                if (positions.Length != comp.sampleCount * 6) positions = new Vector3[comp.sampleCount * 6];
                var prevResult = comp.Evaluate(fromPercent);
                var prevNormal = prevResult.up;
                if (comp.billboardThickness) prevNormal = (editorCamera.position - prevResult.position).normalized;
                var prevRight = Vector3.Cross(prevResult.forward, prevNormal).normalized * prevResult.size * 0.5f;
                var pointIndex = 0;
                for (var i = 1; i < comp.sampleCount; i++)
                {
                    var newResult = comp.samples[i];
                    var newNormal = newResult.up;
                    if (comp.billboardThickness) newNormal = (editorCamera.position - newResult.position).normalized;
                    var newRight = Vector3.Cross(newResult.forward, newNormal).normalized * newResult.size * 0.5f;

                    positions[pointIndex]                        = prevResult.position + prevRight;
                    positions[pointIndex + comp.sampleCount * 2] = prevResult.position - prevRight;
                    positions[pointIndex + comp.sampleCount * 4] = newResult.position  - newRight;
                    pointIndex++;
                    positions[pointIndex]                        = newResult.position + newRight;
                    positions[pointIndex + comp.sampleCount * 2] = newResult.position - newRight;
                    positions[pointIndex + comp.sampleCount * 4] = newResult.position + newRight;
                    pointIndex++;
                    prevResult = newResult;
                    prevRight  = newRight;
                    prevNormal = newNormal;
                }

                Handles.DrawLines(positions);
            }

            Handles.color = prevColor;
        }
    }
}