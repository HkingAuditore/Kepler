using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class PointModule : EditorModule
    {
        public delegate void EmptyHandler();

        public delegate void IntArrayHandler(int[] values);

        public delegate void IntHandler(int value);

        public delegate void UndoHandler(string title);

        public    Spline.Direction duplicationDirection = Spline.Direction.Forward;
        protected SplineEditor     editor;

        protected EditorGUIEvents eventModule;
        public    Color           highlightColor = Color.white;
        private   Vector3         idealPivot     = Vector3.zero;

        private bool movePivot;
        public  bool showPointNumbers = false;


        public PointModule(SplineEditor editor)
        {
            this.editor = editor;
            eventModule = editor.eventModule;
        }

        protected bool isClosed => editor.isClosed;

        protected int sampleRate => editor.sampleRate;

        protected Spline.Type splineType => editor.splineType;

        protected Color color => editor.color;

        protected SplinePoint[] points
        {
            get => editor.points;
            set => editor.points = value;
        }

        protected List<int> selectedPoints
        {
            get => editor.selectedPoints;
            set => editor.selectedPoints = value;
        }

        public Vector3 center
        {
            get
            {
                var avg = Vector3.zero;
                if (points.Length == 0) return avg;
                for (var i = 0; i < points.Length; i++) avg += points[i].position;
                return avg / points.Length;
            }
        }

        public Vector3 selectionCenter
        {
            get
            {
                var avg = Vector3.zero;
                if (selectedPoints.Count == 0) return avg;
                for (var i = 0; i        < selectedPoints.Count; i++) avg += points[selectedPoints[i]].position;
                return avg / selectedPoints.Count;
            }
        }

        public event EmptyHandler    onBeforeDeleteSelectedPoints;
        public event EmptyHandler    onSelectionChanged;
        public event IntArrayHandler onDuplicatePoint;

        protected override void RecordUndo(string title)
        {
            if (editor.undoHandler != null) editor.undoHandler(title);
        }

        protected override void Repaint()
        {
            if (editor.repaintHandler != null) editor.repaintHandler();
        }

        public override void BeforeSceneDraw(SceneView current)
        {
            base.BeforeSceneDraw(current);
            var e = Event.current;

            if (movePivot)
            {
                SceneView.lastActiveSceneView.pivot =
                    Vector3.Lerp(SceneView.lastActiveSceneView.pivot, idealPivot, 0.02f);
                if (e.type == EventType.MouseDown || e.type == EventType.MouseUp) movePivot = false;
                if (Vector3.Distance(SceneView.lastActiveSceneView.pivot, idealPivot) <= 0.05f)
                {
                    SceneView.lastActiveSceneView.pivot = idealPivot;
                    movePivot                           = false;
                }
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && HasSelection())
            {
                DeleteSelectedPoints();
                e.Use();
            }

            if (e.type == EventType.ExecuteCommand && Tools.current == Tool.None)
                switch (e.commandName)
                {
                    case "FrameSelected":
                        if (points.Length > 0)
                        {
                            e.commandName = "";
                            FramePoints();
                            e.Use();
                        }

                        break;
                    case "SelectAll":
                        e.commandName = "";
                        ClearSelection();
                        for (var i = 0; i < points.Length; i++) AddPointSelection(i);
                        e.Use();
                        break;

                    case "Duplicate":
                        if (points.Length > 0 && selectedPoints.Count > 0)
                        {
                            e.commandName = "";
                            DuplicateSelected();
                            e.Use();
                        }

                        break;
                }
        }

        public virtual void DuplicateSelected()
        {
            if (selectedPoints.Count == 0) return;
            RecordUndo("Duplicate Points");
            var newPoints = new SplinePoint[points.Length + selectedPoints.Count];
            var duplicated = new SplinePoint[selectedPoints.Count];
            var index = 0;
            for (var i = 0; i < selectedPoints.Count; i++) duplicated[index++] = points[selectedPoints[i]];
            int min = points.Length - 1, max = 0;
            for (var i = 0; i < selectedPoints.Count; i++)
            {
                if (selectedPoints[i] < min) min = selectedPoints[i];
                if (selectedPoints[i] > max) max = selectedPoints[i];
            }

            var selected = selectedPoints.ToArray();
            selectedPoints.Clear();
            if (duplicationDirection == Spline.Direction.Backward)
            {
                for (var i = 0; i < min; i++) newPoints[i] = points[i];
                for (var i = 0; i < duplicated.Length; i++)
                {
                    newPoints[i + min] = duplicated[i];
                    selectedPoints.Add(i + min);
                }

                for (var i = min; i < points.Length; i++) newPoints[i + duplicated.Length] = points[i];
            }
            else
            {
                for (var i = 0; i <= max; i++) newPoints[i] = points[i];
                for (var i = 0; i < duplicated.Length; i++)
                {
                    newPoints[i + max + 1] = duplicated[i];
                    selectedPoints.Add(i + max + 1);
                }

                for (var i = max + 1; i < points.Length; i++) newPoints[i + duplicated.Length] = points[i];
            }

            points = newPoints;
            if (onDuplicatePoint != null) onDuplicatePoint(selected);
        }

        public virtual void Reset()
        {
        }

        public bool HasSelection()
        {
            return selectedPoints.Count > 0;
        }

        public void ClearSelection()
        {
            selectedPoints.Clear();
            Repaint();
            if (editor.selectionChangeHandler != null) editor.selectionChangeHandler();
            if (onSelectionChanged            != null) onSelectionChanged();
        }

        protected void DeleteSelectedPoints()
        {
            if (onBeforeDeleteSelectedPoints != null) onBeforeDeleteSelectedPoints();

            if (isClosed && selectedPoints.Count == points.Length - 1)
                for (var i = points.Length - 1; i >= 0; i--)
                    DeletePoint(i);
            else
                for (var i = 0; i < selectedPoints.Count; i++)
                {
                    DeletePoint(selectedPoints[i]);
                    for (var n = i; n < selectedPoints.Count; n++) selectedPoints[n]--;
                }

            ClearSelection();
        }

        protected void DeletePoint(int index)
        {
            RecordUndo("Delete Point");
            var p = points;
            UnityEditor.ArrayUtility.RemoveAt(ref p, index);
            points = p;
        }


        public void InverseSelection()
        {
            var inverse = new List<int>();
            for (var i = 0; i < (isClosed ? points.Length - 1 : points.Length); i++)
            {
                var found = false;
                for (var j = 0; j < selectedPoints.Count; j++)
                    if (selectedPoints[j] == i)
                    {
                        found = true;
                        break;
                    }

                if (!found) inverse.Add(i);
            }

            selectedPoints = new List<int>(inverse);
            Repaint();
            if (editor.selectionChangeHandler != null) editor.selectionChangeHandler();
            if (onSelectionChanged            != null) onSelectionChanged();
        }

        protected void SelectPoint(int index)
        {
            if (isClosed && index    == points.Length - 1) return;
            if (selectedPoints.Count == 1 && selectedPoints[0] == index) return;
            selectedPoints.Clear();
            selectedPoints.Add(index);
            Repaint();
            if (editor.selectionChangeHandler != null) editor.selectionChangeHandler();
            if (onSelectionChanged            != null) onSelectionChanged();
        }

        protected void DeselectPoint(int index)
        {
            if (selectedPoints.Contains(index))
            {
                selectedPoints.Remove(index);
                Repaint();
                if (editor.selectionChangeHandler != null) editor.selectionChangeHandler();
                if (onSelectionChanged            != null) onSelectionChanged();
            }
        }

        protected void SelectPoints(List<int> indices)
        {
            selectedPoints.Clear();
            for (var i = 0; i < indices.Count; i++)
            {
                if (isClosed && i == points.Length - 1) continue;
                selectedPoints.Add(indices[i]);
            }

            Repaint();
            if (editor.selectionChangeHandler != null) editor.selectionChangeHandler();
            if (onSelectionChanged            != null) onSelectionChanged();
        }

        protected void AddPointSelection(int index)
        {
            if (isClosed && index == points.Length - 1) return;
            if (selectedPoints.Contains(index)) return;
            selectedPoints.Add(index);
            Repaint();
            if (editor.selectionChangeHandler != null) editor.selectionChangeHandler();
            if (onSelectionChanged            != null) onSelectionChanged();
        }

        protected void FramePoints()
        {
            if (points.Length == 0) return;
            var     center = Vector3.zero;
            var     camera = SceneView.lastActiveSceneView.camera;
            var     cam    = camera.transform;
            Vector3 min    = Vector3.zero, max = Vector3.zero;
            if (HasSelection())
            {
                for (var i = 0; i < selectedPoints.Count; i++)
                {
                    center += points[selectedPoints[i]].position;
                    var local                  = cam.InverseTransformPoint(points[selectedPoints[i]].position);
                    if (local.x < min.x) min.x = local.x;
                    if (local.y < min.y) min.y = local.y;
                    if (local.z < min.z) min.z = local.z;
                    if (local.x > max.x) max.x = local.x;
                    if (local.y > max.y) max.y = local.y;
                    if (local.z > max.z) max.z = local.z;
                }

                center /= selectedPoints.Count;
            }
            else
            {
                for (var i = 0; i < points.Length; i++)
                {
                    center += points[i].position;
                    var local                  = cam.InverseTransformPoint(points[i].position);
                    if (local.x < min.x) min.x = local.x;
                    if (local.y < min.y) min.y = local.y;
                    if (local.z < min.z) min.z = local.z;
                    if (local.x > max.x) max.x = local.x;
                    if (local.y > max.y) max.y = local.y;
                    if (local.z > max.z) max.z = local.z;
                }

                center /= points.Length;
            }

            movePivot  = true;
            idealPivot = center;
        }
    }
}