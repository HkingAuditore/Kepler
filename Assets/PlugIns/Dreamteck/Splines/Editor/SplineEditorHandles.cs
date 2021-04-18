using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public static class SplineEditorHandles
    {
        public static bool SliderButton(Vector3 position, bool drawHandle, Color color, float size)
        {
            var cam      = SceneView.currentDrawingSceneView.camera;
            var localPos = cam.transform.InverseTransformPoint(position);
            if (localPos.z < 0f) return false;

            size *= HandleUtility.GetHandleSize(position);
            var screenPos = HandleUtility.WorldToGUIPoint(position);
            var screenRectBase =
                HandleUtility.WorldToGUIPoint(position - cam.transform.right * size + cam.transform.up * size);
            var rect = new Rect(screenRectBase.x, screenRectBase.y, (screenPos.x - screenRectBase.x) * 2f,
                                (screenPos.y                                     - screenRectBase.y) * 2f);
            if (drawHandle)
            {
                var previousColor = Handles.color;
                Handles.color = color;
                Handles.RectangleHandleCap(0, position, Quaternion.LookRotation(-cam.transform.forward),
                                           HandleUtility.GetHandleSize(position) * 0.1f, EventType.Repaint);
                Handles.color = previousColor;
            }

            if (rect.Contains(Event.current.mousePosition))
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    return true;
            return false;
        }

        public static bool CircleButton(Vector3 position,                Quaternion rotation, float size,
                                        float   clickableAreaMultiplier, Color      color)
        {
            var prev = Handles.color;
            var result = false;
            Handles.color = color;
            result = Handles.Button(position, rotation, size, size * clickableAreaMultiplier, Handles.CircleHandleCap);
            Handles.color = prev;
            return result;
        }

        public static Vector3 FreeMoveRectangle(Vector3 position, float size)
        {
            return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.CircleHandleCap);
        }

        public static Vector3 FreeMoveCircle(Vector3 position, float size)
        {
            return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.CircleHandleCap);
        }

        public static void DrawSolidSphere(Vector3 position, float radius)
        {
            Handles.SphereHandleCap(0, position, Quaternion.identity, radius, EventType.Repaint);
        }

        public static void DrawCircle(Vector3 position, Quaternion rotation, float radius)
        {
            Handles.CircleHandleCap(0, position, rotation, radius, EventType.Repaint);
        }

        public static void DrawRectangle(Vector3 position, Quaternion rotation, float size)
        {
            Handles.RectangleHandleCap(0, position, rotation, size, EventType.Repaint);
        }

        public static void DrawArrowCap(Vector3 position, Quaternion rotation, float size)
        {
            Handles.ArrowHandleCap(0, position, rotation, size, EventType.Repaint);
        }

        public static bool HoverArea(Vector3 position, float size)
        {
            var cam      = SceneView.currentDrawingSceneView.camera;
            var localPos = cam.transform.InverseTransformPoint(position);
            if (localPos.z < 0f) return false;

            size *= HandleUtility.GetHandleSize(position);
            var screenPos = HandleUtility.WorldToGUIPoint(position);
            var screenRectBase =
                HandleUtility.WorldToGUIPoint(position - cam.transform.right * size + cam.transform.up * size);
            var rect = new Rect(screenRectBase.x, screenRectBase.y, (screenPos.x - screenRectBase.x) * 2f,
                                (screenPos.y                                     - screenRectBase.y) * 2f);
            if (rect.Contains(Event.current.mousePosition)) return true;
            return false;
        }
    }
}