using Dreamteck.Editor;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class ComputerEditor : SplineEditorBase
    {
        private readonly SerializedProperty     customNormalInterpolation;
        private readonly SerializedProperty     customValueInterpolation;
        public           bool                   drawComputer           = true;
        public           bool                   drawConnectedComputers = true;
        private readonly SerializedProperty     linearAverageDirection;
        private readonly ComputerEditorModule[] modules = new ComputerEditorModule[0];
        private readonly SerializedProperty     multithreaded;
        private          int                    operation = -1, module = -1, transformTool = 1;
        private readonly Toolbar                operationsToolbar;
        private readonly SerializedProperty     optimizeAngleThreshold;
        private readonly DreamteckSplinesEditor pathEditor;
        private          bool                   pathToolsFoldout, interpolationFoldout;
        private readonly SerializedProperty     rebuildOnAwake;
        private readonly SerializedProperty     sampleMode;
        private readonly SerializedProperty     sampleRate;
        private readonly SerializedObject       serializedObject;
        private readonly SerializedProperty     space;
        private readonly SplineComputer         spline;

        private readonly SerializedProperty splineProperty;
        private readonly SplineComputer[]   splines = new SplineComputer[0];
        private readonly Toolbar            transformToolbar;
        private readonly SerializedProperty type;
        private readonly SerializedProperty updateMode;
        private readonly Toolbar            utilityToolbar;


        public ComputerEditor(SplineComputer[]       splines, SerializedObject serializedObject,
                              DreamteckSplinesEditor pathEditor)
        {
            spline                = splines[0];
            this.splines          = splines;
            this.pathEditor       = pathEditor;
            this.serializedObject = serializedObject;

            splineProperty            = serializedObject.FindProperty("spline");
            sampleRate                = serializedObject.FindProperty("spline").FindPropertyRelative("sampleRate");
            type                      = serializedObject.FindProperty("spline").FindPropertyRelative("type");
            linearAverageDirection    = splineProperty.FindPropertyRelative("linearAverageDirection");
            space                     = serializedObject.FindProperty("_space");
            sampleMode                = serializedObject.FindProperty("_sampleMode");
            optimizeAngleThreshold    = serializedObject.FindProperty("_optimizeAngleThreshold");
            updateMode                = serializedObject.FindProperty("updateMode");
            rebuildOnAwake            = serializedObject.FindProperty("rebuildOnAwake");
            multithreaded             = serializedObject.FindProperty("multithreaded");
            customNormalInterpolation = splineProperty.FindPropertyRelative("customNormalInterpolation");
            customValueInterpolation  = splineProperty.FindPropertyRelative("customValueInterpolation");


            modules    = new ComputerEditorModule[2];
            modules[0] = new ComputerMergeModule(spline);
            modules[1] = new ComputerSplitModule(spline);
            GUIContent[] utilityContents = new GUIContent[modules.Length],
                utilityContentsSelected  = new GUIContent[modules.Length];
            for (var i = 0; i < modules.Length; i++)
            {
                utilityContents[i]         =  modules[i].GetIconOff();
                utilityContentsSelected[i] =  modules[i].GetIconOn();
                modules[i].undoHandler     += OnRecordUndo;
                modules[i].repaintHandler  += OnRepaint;
            }

            utilityToolbar         = new Toolbar(utilityContents, utilityContentsSelected, 35f);
            utilityToolbar.newLine = false;


            var          index             = 0;
            GUIContent[] transformContents = new GUIContent[4], transformContentsSelected = new GUIContent[4];
            transformContents[index]           = new GUIContent("OFF");
            transformContentsSelected[index++] = new GUIContent("OFF");

            transformContents[index]           = EditorGUIUtility.IconContent("MoveTool");
            transformContentsSelected[index++] = EditorGUIUtility.IconContent("MoveTool On");

            transformContents[index]           = EditorGUIUtility.IconContent("RotateTool");
            transformContentsSelected[index++] = EditorGUIUtility.IconContent("RotateTool On");

            transformContents[index]         = EditorGUIUtility.IconContent("ScaleTool");
            transformContentsSelected[index] = EditorGUIUtility.IconContent("ScaleTool On");

            transformToolbar         = new Toolbar(transformContents, transformContentsSelected, 35f);
            transformToolbar.newLine = false;

            index = 0;
            GUIContent[] operationContents = new GUIContent[3], operationContentsSelected = new GUIContent[3];
            for (var i = 0; i < operationContents.Length; i++)
            {
                operationContents[i]         = new GUIContent("");
                operationContentsSelected[i] = new GUIContent("");
            }

            operationsToolbar         = new Toolbar(operationContents, operationContentsSelected, 64f);
            operationsToolbar.newLine = false;
        }

        private void OnRecordUndo(string title)
        {
            if (undoHandler != null) undoHandler(title);
        }

        private void OnRepaint()
        {
            if (repaintHandler != null) repaintHandler();
        }

        protected override void Load()
        {
            base.Load();
            pathToolsFoldout     = LoadBool("DreamteckSplinesEditor.pathToolsFoldout");
            interpolationFoldout = LoadBool("DreamteckSplinesEditor.interpolationFoldout");
            transformTool        = LoadInt("DreamteckSplinesEditor.transformTool");
        }

        protected override void Save()
        {
            base.Save();
            SaveBool("DreamteckSplinesEditor.pathToolsFoldout",     pathToolsFoldout);
            SaveBool("DreamteckSplinesEditor.interpolationFoldout", interpolationFoldout);
            SaveInt("DreamteckSplinesEditor.transformTool", transformTool);
        }

        public override void Destroy()
        {
            base.Destroy();
            for (var i = 0; i < modules.Length; i++) modules[i].Deselect();
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (spline == null) return;
            SplineEditorGUI.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            operationsToolbar.SetContent(0, new GUIContent(spline.isClosed ? "Break" : "Close"));
            operationsToolbar.SetContent(1, new GUIContent("Reverse"));
            operationsToolbar.SetContent(2, new GUIContent(spline.is2D ? "3D Mode" : "2D Mode"));
            operationsToolbar.Draw(ref operation);
            if (EditorGUI.EndChangeCheck()) PerformOperation();
            EditorGUI.BeginChangeCheck();
            if (splines.Length == 1)
            {
                var mod = module;
                utilityToolbar.Draw(ref mod);
                if (EditorGUI.EndChangeCheck()) ToggleModule(mod);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (module >= 0 && module < modules.Length) modules[module].DrawInspector();
            EditorGUILayout.Space();
            DreamteckEditorGUI.DrawSeparator();

            EditorGUILayout.Space();

            serializedObject.Update();


            EditorGUI.BeginChangeCheck();
            var lastType = (Spline.Type) type.intValue;
            EditorGUILayout.PropertyField(type);
            if (lastType == Spline.Type.CatmullRom && type.intValue == (int) Spline.Type.Bezier)
                if (EditorUtility.DisplayDialog("Hermite to Bezier",
                                                "Would you like to retain the Catmull Rom shape in Bezier mode?", "Yes",
                                                "No"))
                {
                    for (var i = 0; i < splines.Length; i++) splines[i].CatToBezierTangents();

                    serializedObject.Update();
                    pathEditor.Refresh();
                }

            if (spline.type == Spline.Type.Linear) EditorGUILayout.PropertyField(linearAverageDirection);
            var lastSpace = space.intValue;
            EditorGUILayout.PropertyField(space,      new GUIContent("Space"));
            EditorGUILayout.PropertyField(sampleMode, new GUIContent("Sample Mode"));
            if (sampleMode.intValue == (int) SplineComputer.SampleMode.Optimized)
                EditorGUILayout.PropertyField(optimizeAngleThreshold);
            EditorGUILayout.PropertyField(updateMode);
            if (updateMode.intValue == (int) SplineComputer.UpdateMode.None && Application.isPlaying)
                if (GUILayout.Button("Rebuild"))
                    for (var i = 0; i < splines.Length; i++)
                        splines[i].RebuildImmediate(true, true);
            if (spline.type != Spline.Type.Linear)
                EditorGUILayout.PropertyField(sampleRate, new GUIContent("Sample Rate"));
            EditorGUILayout.PropertyField(rebuildOnAwake);
            EditorGUILayout.PropertyField(multithreaded);

            EditorGUI.indentLevel++;
            var curveUpdate = false;
            interpolationFoldout = EditorGUILayout.Foldout(interpolationFoldout, "Point Value Interpolation");
            if (interpolationFoldout)
            {
                if (customValueInterpolation.animationCurveValue             == null ||
                    customValueInterpolation.animationCurveValue.keys.Length == 0)
                {
                    if (GUILayout.Button("Size & Color Interpolation"))
                    {
                        var curve = new AnimationCurve();
                        curve.AddKey(new Keyframe(0, 0, 0, 0));
                        curve.AddKey(new Keyframe(1, 1, 0, 0));
                        for (var i = 0; i < splines.Length; i++) splines[i].customValueInterpolation = curve;
                        serializedObject.Update();
                        curveUpdate = true;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(customValueInterpolation,
                                                  new GUIContent("Size & Color Interpolation"));
                    if (GUILayout.Button("x", GUILayout.MaxWidth(25)))
                    {
                        customValueInterpolation.animationCurveValue = null;
                        for (var i = 0; i < splines.Length; i++) splines[i].customValueInterpolation = null;
                        serializedObject.Update();
                        curveUpdate = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (customNormalInterpolation.animationCurveValue             == null ||
                    customNormalInterpolation.animationCurveValue.keys.Length == 0)
                {
                    if (GUILayout.Button("Normal Interpolation"))
                    {
                        var curve = new AnimationCurve();
                        curve.AddKey(new Keyframe(0, 0));
                        curve.AddKey(new Keyframe(1, 1));
                        for (var i = 0; i < splines.Length; i++) splines[i].customNormalInterpolation = curve;
                        serializedObject.Update();
                        curveUpdate = true;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(customNormalInterpolation, new GUIContent("Normal Interpolation"));
                    if (GUILayout.Button("x", GUILayout.MaxWidth(25)))
                    {
                        customNormalInterpolation.animationCurveValue = null;
                        for (var i = 0; i < splines.Length; i++) splines[i].customNormalInterpolation = null;
                        serializedObject.Update();
                        curveUpdate = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck() || curveUpdate)
            {
                if (sampleRate.intValue < 2) sampleRate.intValue = 2;
                if (lastSpace != space.intValue)
                {
                    for (var i = 0; i < splines.Length; i++) splines[i].space = (SplineComputer.Space) space.intValue;
                    serializedObject.Update();
                    if (splines.Length == 1) pathEditor.Refresh();
                }

                serializedObject.ApplyModifiedProperties();
                for (var i = 0; i < splines.Length; i++) splines[i].Rebuild(true);
            }


            if (pathEditor.currentModule != null) transformTool = 0;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Edit Transform");
            GUILayout.FlexibleSpace();
            var lastTool = transformTool;
            transformToolbar.Draw(ref transformTool);
            if (lastTool != transformTool && transformTool > 0) pathEditor.UntoggleCurrentModule();
            EditorGUILayout.EndHorizontal();
        }

        private void PerformOperation()
        {
            switch (operation)
            {
                case 0:
                    if (spline.isClosed) BreakSpline();
                    else CloseSpline();
                    operation = -1;
                    break;
                case 1:
                    ReversePointOrder();
                    operation = -1;
                    break;
                case 2:
                    spline.is2D = !spline.is2D;
                    operation   = -1;
                    break;
            }
        }

        private void ToggleModule(int index)
        {
            if (module >= 0 && module < modules.Length) modules[module].Deselect();
            if (module == index) index = -1;
            module = index;
            if (module >= 0 && module < modules.Length) modules[module].Select();
        }

        public void BreakSpline()
        {
            RecordUndo("Break path");
            if (splines.Length == 1 && pathEditor.selectedPoints.Count == 1) spline.Break(pathEditor.selectedPoints[0]);
            else
                for (var i = 0; i < splines.Length; i++)
                    splines[i].Break();
        }

        public void CloseSpline()
        {
            RecordUndo("Close path");
            for (var i = 0; i < splines.Length; i++) splines[i].Close();
        }

        private void ReversePointOrder()
        {
            for (var i = 0; i < splines.Length; i++) ReversePointOrder(splines[i]);
        }

        private void ReversePointOrder(SplineComputer spline)
        {
            var points = spline.GetPoints();
            for (var i = 0; i < Mathf.FloorToInt(points.Length / 2); i++)
            {
                var temp = points[i];
                points[i] = points[points.Length - 1 - i];
                var tempTan = points[i].tangent;
                points[i].tangent  = points[i].tangent2;
                points[i].tangent2 = tempTan;
                var opposideIndex = points.Length - 1 - i;
                points[opposideIndex]          = temp;
                tempTan                        = points[opposideIndex].tangent;
                points[opposideIndex].tangent  = points[opposideIndex].tangent2;
                points[opposideIndex].tangent2 = tempTan;
            }

            if (points.Length % 2 != 0)
            {
                var tempTan = points[Mathf.CeilToInt(points.Length / 2)].tangent;
                points[Mathf.CeilToInt(points.Length     / 2)].tangent =
                    points[Mathf.CeilToInt(points.Length / 2)].tangent2;
                points[Mathf.CeilToInt(points.Length / 2)].tangent2 = tempTan;
            }

            spline.SetPoints(points);
        }

        public override void DrawScene(SceneView current)
        {
            base.DrawScene(current);
            if (drawComputer)
                for (var i = 0; i < splines.Length; i++)
                    DSSplineDrawer.DrawSplineComputer(splines[i]);
            if (drawConnectedComputers)
                for (var i = 0; i < splines.Length; i++)
                {
                    var computers = splines[i].GetConnectedComputers();
                    for (var j = 1; j < computers.Count; j++)
                        DSSplineDrawer.DrawSplineComputer(computers[j], 0.0, 1.0, 0.5f);
                }


            if (pathEditor.currentModule == null)
            {
                switch (transformTool)
                {
                    case 1:
                        for (var i = 0; i < splines.Length; i++)
                        {
                            var position = splines[i].transform.position;
                            position = Handles.PositionHandle(position, splines[i].transform.rotation);
                            if (position != splines[i].transform.position)
                            {
                                RecordUndo("Move spline computer");
                                Undo.RecordObject(splines[i].transform, "Move spline computer");
                                splines[i].transform.position = position;
                                splines[i].SetPoints(pathEditor.points);
                                pathEditor.Refresh();
                            }
                        }

                        break;
                    case 2:
                        for (var i = 0; i < splines.Length; i++)
                        {
                            var rotation = splines[i].transform.rotation;
                            rotation = Handles.RotationHandle(rotation, splines[i].transform.position);
                            if (rotation != splines[i].transform.rotation)
                            {
                                RecordUndo("Rotate spline computer");
                                Undo.RecordObject(splines[i].transform, "Rotate spline computer");
                                splines[i].transform.rotation = rotation;
                                splines[i].SetPoints(pathEditor.points);
                                pathEditor.Refresh();
                            }
                        }

                        break;
                    case 3:
                        for (var i = 0; i < splines.Length; i++)
                        {
                            var scale = splines[i].transform.localScale;
                            scale = Handles.ScaleHandle(scale, splines[i].transform.position,
                                                        splines[i].transform.rotation,
                                                        HandleUtility.GetHandleSize(splines[i].transform.position));
                            if (scale != splines[i].transform.localScale)
                            {
                                RecordUndo("Scale spline computer");
                                Undo.RecordObject(splines[i].transform, "Scale spline computer");
                                splines[i].transform.localScale = scale;
                                splines[i].SetPoints(pathEditor.points);
                                pathEditor.Refresh();
                            }
                        }

                        break;
                }

                if (transformTool > 0)
                    for (var i = 0; i < splines.Length; i++)
                    {
                        var screenPosition = HandleUtility.WorldToGUIPoint(splines[i].transform.position);
                        screenPosition.y += 20f;
                        Handles.BeginGUI();
                        DreamteckEditorGUI
                           .Label(new Rect(screenPosition.x - 120 + splines[i].name.Length * 4, screenPosition.y, 120, 25),
                                  splines[i].name);
                        Handles.EndGUI();
                    }
            }

            if (module >= 0 && module < modules.Length) modules[module].DrawScene();
        }
    }
}