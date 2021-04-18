using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public static class SplineComputerEditorHandles
    {
        public enum SplineSliderGizmo
        {
            ForwardTriangle,
            BackwardTriangle,
            DualArrow,
            Rectangle,
            Circle
        }

        private static readonly SplineSample evalResult = new SplineSample();

        public static bool Slider(SplineComputer    spline, ref double percent, Color color, string text = "",
                                  SplineSliderGizmo gizmo = SplineSliderGizmo.Rectangle, float buttonSize = 1f)
        {
            var cam = SceneView.currentDrawingSceneView.camera;
            spline.Evaluate(percent, evalResult);
            var size = HandleUtility.GetHandleSize(evalResult.position);

            Handles.color = new Color(color.r, color.g, color.b, 0.4f);
            Handles.DrawSolidDisc(evalResult.position, cam.transform.position - evalResult.position,
                                  size * 0.2f * buttonSize);
            Handles.color = Color.white;
            if ((color.r + color.g + color.b + color.a) / 4f >= 0.9f) Handles.color = Color.black;

            var center         = evalResult.position;
            var screenPosition = HandleUtility.WorldToGUIPoint(center);
            screenPosition.y += 20f;
            var localPos = cam.transform.InverseTransformPoint(center);
            if (text != "" && localPos.z > 0f)
            {
                Handles.BeginGUI();
                DreamteckEditorGUI.Label(new Rect(screenPosition.x - 120 + text.Length * 4, screenPosition.y, 120, 25),
                                         text);
                Handles.EndGUI();
            }

            var buttonClick  = SplineEditorHandles.SliderButton(center, false, Color.white, 0.3f);
            var lookAtCamera = (cam.transform.position - evalResult.position).normalized;
            var right        = Vector3.Cross(lookAtCamera, evalResult.forward).normalized * size * 0.1f * buttonSize;
            var front        = Vector3.forward;
            switch (gizmo)
            {
                case SplineSliderGizmo.BackwardTriangle:
                    center += evalResult.forward * size * 0.06f * buttonSize;
                    front  =  center - evalResult.forward * size * 0.2f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front,          center - right);
                    Handles.DrawLine(center                 - right, center + right);
                    break;

                case SplineSliderGizmo.ForwardTriangle:
                    center -= evalResult.forward * size * 0.06f * buttonSize;
                    front  =  center + evalResult.forward * size * 0.2f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front,          center - right);
                    Handles.DrawLine(center                 - right, center + right);
                    break;

                case SplineSliderGizmo.DualArrow:
                    center += evalResult.forward * size * 0.025f * buttonSize;
                    front  =  center + evalResult.forward * size * 0.17f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front,          center - right);
                    Handles.DrawLine(center                 - right, center + right);
                    center -= evalResult.forward * size * 0.05f * buttonSize;
                    front  =  center - evalResult.forward * size * 0.17f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front,          center - right);
                    Handles.DrawLine(center                 - right, center + right);
                    break;
                case SplineSliderGizmo.Rectangle:

                    break;

                case SplineSliderGizmo.Circle:
                    Handles.DrawWireDisc(center, lookAtCamera, 0.13f * size * buttonSize);
                    break;
            }

            var lastPos = evalResult.position;
            Handles.color = Color.clear;
            evalResult.position = Handles.FreeMoveHandle(evalResult.position,
                                                         Quaternion.LookRotation(cam.transform.position -
                                                                                 evalResult.position),
                                                         size * 0.2f * buttonSize, Vector3.zero,
                                                         Handles.CircleHandleCap);
            if (evalResult.position != lastPos) percent = spline.Project(evalResult.position).percent;
            Handles.color = Color.white;
            return buttonClick;
        }
    }
}