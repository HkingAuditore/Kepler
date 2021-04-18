using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class MainPointModule : PointModule
    {
        private bool    drag;
        public  bool    excludeSelected = false;
        private bool    finalize;
        public  int     minimumRectSize = 5;
        private bool    pointsMoved;
        private Rect    rect;
        private Vector2 rectEnd   = Vector2.zero;
        private Vector2 rectStart = Vector2.zero;

        public MainPointModule(SplineEditor editor) : base(editor)
        {
        }

        public bool isDragging => drag && rect.width >= minimumRectSize && rect.height >= minimumRectSize;

        public override void DrawInspector()
        {
            var pointCount = isClosed ? points.Length - 1 : points.Length;
            var options    = new string[pointCount    + 4];
            options[0] = "- - -";
            if (selectedPoints.Count > 1) options[0] = "- Multiple -";
            options[1] = "All";
            options[2] = "None";
            options[3] = "Inverse";
            for (var i = 0; i < pointCount; i++)
            {
                options[i + 4] = "Point " + (i + 1);
                if (splineType == Spline.Type.Bezier)
                    switch (points[i].type)
                    {
                        case SplinePoint.Type.Broken:
                            options[i + 4] += " - Broken";
                            break;
                        case SplinePoint.Type.SmoothFree:
                            options[i + 4] += " - Smooth Free";
                            break;
                        case SplinePoint.Type.SmoothMirrored:
                            options[i + 4] += " - Smooth Mirrored";
                            break;
                    }
            }

            var option                            = 0;
            if (selectedPoints.Count == 1) option = selectedPoints[0] + 4;
            option = EditorGUILayout.Popup("Select", option, options);
            switch (option)
            {
                case 1:
                    ClearSelection();
                    for (var i = 0; i < points.Length; i++) AddPointSelection(i);
                    break;

                case 2:
                    ClearSelection();
                    break;

                case 3:
                    InverseSelection();
                    break;
            }

            if (option >= 4) SelectPoint(option - 4);
        }

        public override void DrawScene()
        {
            if (eventModule.v) return;
            var camTransform = SceneView.currentDrawingSceneView.camera.transform;
            if (!drag)
            {
                if (finalize)
                {
                    if (rect.width > 0f && rect.height > 0f)
                    {
                        if (!eventModule.control) ClearSelection();
                        for (var i = 0; i < points.Length; i++)
                        {
                            var guiPoint = HandleUtility.WorldToGUIPoint(points[i].position);
                            if (rect.Contains(guiPoint))
                            {
                                var local = camTransform.InverseTransformPoint(points[i].position);
                                if (local.z >= 0f) AddPointSelection(i);
                            }
                        }
                    }

                    finalize = false;
                }
            }
            else
            {
                rectEnd = Event.current.mousePosition;
                rect = new Rect(Mathf.Min(rectStart.x, rectEnd.x), Mathf.Min(rectStart.y, rectEnd.y),
                                Mathf.Abs(rectEnd.x - rectStart.x), Mathf.Abs(rectEnd.y - rectStart.y));
                if (rect.width >= minimumRectSize && rect.height >= minimumRectSize)
                {
                    var col = highlightColor;
                    col.a = 0.4f;
                    Handles.BeginGUI();
                    EditorGUI.DrawRect(rect, col);
                    Handles.EndGUI();
                    SceneView.RepaintAll();
                }
            }

            var originalAlignment = GUI.skin.label.alignment;
            var originalColor     = GUI.skin.label.normal.textColor;

            GUI.skin.label.alignment        = TextAnchor.MiddleCenter;
            GUI.skin.label.normal.textColor = color;

            for (var i = 0; i < points.Length; i++)
            {
                if (isClosed && i == points.Length - 1) break;
                var isSelected = selectedPoints.Contains(i);
                var lastPos = points[i].position;
                Handles.color = Color.clear;
                if (showPointNumbers && camTransform.InverseTransformPoint(points[i].position).z > 0f)
                    Handles.Label(points[i].position + Camera.current.transform.up * HandleUtility.GetHandleSize(points[i].position) * 0.3f,
                                  (i + 1).ToString());
                if (excludeSelected && isSelected)
                    SplineEditorHandles.FreeMoveRectangle(points[i].position,
                                                          HandleUtility.GetHandleSize(points[i].position) * 0.1f);
                else
                    points[i].SetPosition(SplineEditorHandles.FreeMoveRectangle(points[i].position,
                                                                                HandleUtility
                                                                                   .GetHandleSize(points[i].position) *
                                                                                0.1f));

                if (lastPos != points[i].position)
                {
                    RecordUndo("Move Points");
                    pointsMoved = true;
                    if (isSelected)
                        for (var n = 0; n < selectedPoints.Count; n++)
                        {
                            if (selectedPoints[n] == i) continue;
                            points[selectedPoints[n]]
                               .SetPosition(points[selectedPoints[n]].position + (points[i].position - lastPos));
                        }
                    else SelectPoint(i);

                    lastPos = points[i].position;
                }


                if (!pointsMoved && !eventModule.alt && editor.eventModule.mouseLeftUp)
                    if (SplineEditorHandles.HoverArea(points[i].position, 0.12f))
                    {
                        if (eventModule.control && selectedPoints.Contains(i))
                        {
                            DeselectPoint(i);
                        }
                        else
                        {
                            if (eventModule.shift) ShiftSelect(i, points.Length);
                            else if (eventModule.control) AddPointSelection(i);
                            else SelectPoint(i);
                        }
                    }

                if (!excludeSelected || !isSelected)
                {
                    Handles.color = color;
                    if (isSelected)
                    {
                        Handles.color = highlightColor;
                        Handles.DrawWireDisc(points[i].position,
                                             -SceneView.currentDrawingSceneView.camera.transform.forward,
                                             HandleUtility.GetHandleSize(points[i].position) * 0.14f);
                    }
                    else
                    {
                        Handles.color = color;
                    }

                    Handles.DrawSolidDisc(points[i].position,
                                          -SceneView.currentDrawingSceneView.camera.transform.forward,
                                          HandleUtility.GetHandleSize(points[i].position) * 0.09f);
                    Handles.color = Color.white;
                }
            }

            GUI.skin.label.alignment        = originalAlignment;
            GUI.skin.label.normal.textColor = originalColor;

            if (splineType == Spline.Type.Bezier)
            {
                Handles.color = color;
                for (var i = 0; i < selectedPoints.Count; i++)
                {
                    Handles.DrawDottedLine(points[selectedPoints[i]].position, points[selectedPoints[i]].tangent,  4f);
                    Handles.DrawDottedLine(points[selectedPoints[i]].position, points[selectedPoints[i]].tangent2, 4f);
                    var lastPos = points[selectedPoints[i]].tangent;
                    var newPos = SplineEditorHandles.FreeMoveCircle(points[selectedPoints[i]].tangent,
                                                                    HandleUtility
                                                                       .GetHandleSize(points[selectedPoints[i]]
                                                                                         .tangent) * 0.1f);
                    if (lastPos != newPos)
                    {
                        RecordUndo("Move Tangent");
                        points[selectedPoints[i]].SetTangentPosition(newPos);
                    }

                    lastPos = points[selectedPoints[i]].tangent2;
                    newPos = SplineEditorHandles.FreeMoveCircle(points[selectedPoints[i]].tangent2,
                                                                HandleUtility.GetHandleSize(points[selectedPoints[i]]
                                                                                               .tangent2) * 0.1f);
                    if (lastPos != newPos)
                    {
                        RecordUndo("Move Tangent");
                        points[selectedPoints[i]].SetTangent2Position(newPos);
                    }
                }
            }

            if (isDragging)
                if (eventModule.alt                                                                           ||
                    !SceneView.currentDrawingSceneView.camera.pixelRect.Contains(Event.current.mousePosition) ||
                    !eventModule.mouseLeft)
                    FinishDrag();
            if (eventModule.mouseLeftUp) pointsMoved = false;
        }

        private void ShiftSelect(int index, int pointCount)
        {
            if (selectedPoints.Count == 0)
            {
                AddPointSelection(index);
                return;
            }

            int minSelected = pointCount - 1, maxSelected = 0;
            for (var i = 0; i < selectedPoints.Count; i++)
            {
                if (minSelected > selectedPoints[i]) minSelected = selectedPoints[i];
                if (maxSelected < selectedPoints[i]) maxSelected = selectedPoints[i];
            }

            if (index > maxSelected)
                for (var i = maxSelected + 1; i <= index; i++)
                    AddPointSelection(i);
            else if (index < minSelected)
                for (var i = minSelected - 1; i >= index; i--)
                    AddPointSelection(i);
            else
                for (var i = minSelected + 1; i <= index; i++)
                    AddPointSelection(i);
        }

        public void StartDrag(Vector2 position)
        {
            rectStart = position;
            drag      = true;
            finalize  = false;
        }

        public void FinishDrag()
        {
            if (!drag) return;
            drag     = false;
            finalize = true;
        }

        public void CancelDrag()
        {
            drag     = false;
            finalize = false;
        }
    }
}