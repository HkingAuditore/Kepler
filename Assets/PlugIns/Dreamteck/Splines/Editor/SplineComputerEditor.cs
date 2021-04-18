using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplineComputer), true)]
    [CanEditMultipleObjects]
    public class SplineComputerEditor : UnityEditor.Editor
    {
        public static bool             hold;
        public        bool             mouseLeft;
        public        bool             mouseRight;
        public        bool             mouseLeftDown;
        public        bool             mouseRightDown;
        public        bool             mouseLeftUp;
        public        bool             mouserightUp;
        public        bool             control;
        public        bool             shift;
        public        bool             alt;
        public        SplineComputer   spline;
        public        SplineComputer[] splines = new SplineComputer[0];


        protected bool              closedOnMirror = false;
        private   ComputerEditor    computerEditor;
        private   SplineDebugEditor debugEditor;

        private          DreamteckSplinesEditor pathEditor;
        private readonly List<int>              selectedPoints = new List<int>();
        private          SplineTriggersEditor   triggersEditor;

        public int[] pointSelection => selectedPoints.ToArray();

        public int selectedPointsCount
        {
            get => selectedPoints.Count;
            set { }
        }

        private void OnEnable()
        {
            splines = new SplineComputer[targets.Length];
            for (var i = 0; i < splines.Length; i++)
            {
                splines[i] = (SplineComputer) targets[i];
                splines[i].EditorAwake();
                if (splines[i].alwaysDraw) DSSplineDrawer.RegisterComputer(splines[i]);
            }

            spline = splines[0];
            InitializeSplineEditor();
            InitializeComputerEditor();
            debugEditor                   =  new SplineDebugEditor(spline);
            debugEditor.undoHandler       += RecordUndo;
            debugEditor.repaintHandler    += OnRepaint;
            triggersEditor                =  new SplineTriggersEditor(spline);
            triggersEditor.undoHandler    += RecordUndo;
            triggersEditor.repaintHandler += OnRepaint;
            hold                          =  false;
#if UNITY_2019_1_OR_NEWER
            SceneView.beforeSceneGui += BeforeSceneGUI;
#else
            SceneView.onSceneGUIDelegate += BeforeSceneGUI;
#endif
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
#if UNITY_2019_1_OR_NEWER
            SceneView.beforeSceneGui -= BeforeSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= BeforeSceneGUI;
#endif
            pathEditor.Destroy();
            computerEditor.Destroy();
            debugEditor.Destroy();
            triggersEditor.Destroy();
        }

        private void OnSceneGUI()
        {
            spline = (SplineComputer) target;
            var currentSceneView = SceneView.currentDrawingSceneView;
            debugEditor.DrawScene(currentSceneView);
            computerEditor.drawComputer = !(pathEditor.currentModule is CreatePointModule);
            computerEditor.DrawScene(currentSceneView);
            if (splines.Length == 1 && triggersEditor.open) triggersEditor.DrawScene(currentSceneView);
            if (splines.Length == 1 && pathEditor.open) pathEditor.DrawScene(currentSceneView);
        }


        [MenuItem("GameObject/3D Object/Spline Computer")]
        private static void NewEmptySpline()
        {
            var count              = FindObjectsOfType<SplineComputer>().Length;
            var objName            = "Spline";
            if (count > 0) objName += " " + count;
            var obj                = new GameObject(objName);
            obj.AddComponent<SplineComputer>();
            if (Selection.activeGameObject != null)
                if (EditorUtility.DisplayDialog("Make child?",
                                                "Do you want to make the new spline a child of " +
                                                Selection.activeGameObject.name                  + "?", "Yes", "No"))
                {
                    obj.transform.parent        = Selection.activeGameObject.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                }

            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/3D Object/Spline Node")]
        private static void NewSplineNode()
        {
            var count              = FindObjectsOfType<Node>().Length;
            var objName            = "Node";
            if (count > 0) objName += " " + count;
            var obj                = new GameObject(objName);
            obj.AddComponent<Node>();
            if (Selection.activeGameObject != null)
                if (EditorUtility.DisplayDialog("Make child?",
                                                "Do you want to make the new node a child of " +
                                                Selection.activeGameObject.name                + "?", "Yes", "No"))
                {
                    obj.transform.parent        = Selection.activeGameObject.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                }

            Selection.activeGameObject = obj;
        }

        public void UndoRedoPerformed()
        {
            pathEditor.points = spline.GetPoints();
            pathEditor.UndoRedoPerformed();
            spline.EditorUpdateConnectedNodes();
            spline.Rebuild();
        }

        private void BeforeSceneGUI(SceneView current)
        {
            pathEditor.BeforeSceneGUI(current);
        }

        private void InitializeSplineEditor()
        {
            pathEditor                = new DreamteckSplinesEditor(spline, "DreamteckSplines");
            pathEditor.undoHandler    = RecordUndo;
            pathEditor.repaintHandler = OnRepaint;
            pathEditor.space          = (SplineEditor.Space) SplinePrefs.pointEditSpace;
        }

        private void InitializeComputerEditor()
        {
            computerEditor                = new ComputerEditor(splines, serializedObject, pathEditor);
            computerEditor.undoHandler    = RecordUndo;
            computerEditor.repaintHandler = OnRepaint;
        }

        private void RecordUndo(string title)
        {
            for (var i = 0; i < splines.Length; i++) Undo.RecordObject(splines[i], title);
        }

        private void OnRepaint()
        {
            SceneView.RepaintAll();
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            spline = (SplineComputer) target;
            Undo.RecordObject(spline, "Edit Points");

            if (splines.Length == 1)
            {
                SplineEditorGUI.BeginContainerBox(ref pathEditor.open, "Edit");
                if (pathEditor.open)
                {
                    var lastSpace = pathEditor.space;
                    pathEditor.DrawInspector();
                    if (lastSpace != pathEditor.space)
                    {
                        SplinePrefs.pointEditSpace = (SplineComputer.Space) pathEditor.space;
                        SplinePrefs.SavePrefs();
                    }
                }
                else if (pathEditor.lastEditorTool != Tool.None && Tools.current == Tool.None)
                {
                    Tools.current = pathEditor.lastEditorTool;
                }

                SplineEditorGUI.EndContainerBox();
            }

            SplineEditorGUI.BeginContainerBox(ref computerEditor.open, "Spline Computer");
            if (computerEditor.open) computerEditor.DrawInspector();
            SplineEditorGUI.EndContainerBox();

            if (splines.Length == 1)
            {
                SplineEditorGUI.BeginContainerBox(ref triggersEditor.open, "Triggers");
                if (triggersEditor.open) triggersEditor.DrawInspector();
                SplineEditorGUI.EndContainerBox();

                SplineEditorGUI.BeginContainerBox(ref debugEditor.open, "Editor Properties");
                if (debugEditor.open) debugEditor.DrawInspector();
                SplineEditorGUI.EndContainerBox();
            }

            if (GUI.changed)
            {
                if (spline.isClosed) pathEditor.points[pathEditor.points.Length - 1] = pathEditor.points[0];
                EditorUtility.SetDirty(spline);
            }
        }


        public bool IsPointSelected(int index)
        {
            return selectedPoints.Contains(index);
        }
    }
}