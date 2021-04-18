using System;
using Dreamteck.Editor;
using Dreamteck.Splines.Primitives;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class PrimitivesModule : PointTransformModule
    {
        private          bool                   createPresetMode;
        private readonly DreamteckSplinesEditor dsEditor;

        private bool              lastClosed;
        private Spline.Type       lastType = Spline.Type.Bezier;
        private int               mode, selectedPrimitive, selectedPreset;
        private string[]          presetNames;
        private SplinePreset[]    presets;
        private PrimitiveEditor[] primitiveEditors;
        private string[]          primitiveNames;

        private          string       savePresetName = "", savePresetDescription = "";
        private readonly Toolbar      toolbar;
        private readonly GUIContent[] toolbarContents = new GUIContent[2];


        public PrimitivesModule(SplineEditor editor) : base(editor)
        {
            dsEditor           = (DreamteckSplinesEditor) editor;
            toolbarContents[0] = new GUIContent("Primitives", "Procedural Primitives");
            toolbarContents[1] = new GUIContent("Presets",    "Saved spline presets");
            toolbar            = new Toolbar(toolbarContents, toolbarContents);
        }

        public override GUIContent GetIconOff()
        {
            return IconContent("*", "primitives", "Spline Primitives");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("*", "primitives_on", "Spline Primitives");
        }

        public override void LoadState()
        {
            base.LoadState();
            selectedPrimitive = LoadInt("selectedPrimitive");
            mode              = LoadInt("mode");
            createPresetMode  = LoadBool("createPresetMode");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveInt("selectedPrimitive", selectedPrimitive);
            SaveInt("mode",              mode);
            SaveBool("createPresetMode", createPresetMode);
        }

        public override void Select()
        {
            base.Select();
            lastClosed = editor.isClosed;
            lastType   = editor.splineType;
            Apply();
            if (mode == 0) LoadPrimitives();
            else if (!createPresetMode) LoadPresets();
        }

        public override void Deselect()
        {
            base.Deselect();
            ApplyDialog();
        }

        private void ApplyDialog()
        {
            if (!IsDirty()) return;
            if (EditorUtility.DisplayDialog("Unapplied Primitives",
                                            "There is an unapplied primitive. Do you want to apply the changes?",
                                            "Apply", "Revert")) Apply();
            else Revert();
        }

        public override void Revert()
        {
            base.Revert();
            editor.splineType = lastType;
            editor.isClosed   = lastClosed;
        }

        public override void DrawInspector()
        {
            EditorGUI.BeginChangeCheck();
            toolbar.Draw(ref mode);
            if (EditorGUI.EndChangeCheck())
            {
                if (mode == 0) LoadPrimitives();
                else if (!createPresetMode) LoadPresets();
            }

            if (selectedPoints.Count > 0) ClearSelection();
            if (mode == 0) PrimitivesGUI();
            else PresetsGUI();

            if (IsDirty() && (!createPresetMode || mode == 0))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply")) Apply();
                if (GUILayout.Button("Revert")) Revert();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void PrimitivesGUI()
        {
            var last = selectedPrimitive;
            selectedPrimitive = EditorGUILayout.Popup(selectedPrimitive, primitiveNames);
            if (last != selectedPrimitive)
            {
                primitiveEditors[selectedPrimitive].Open(dsEditor);
                primitiveEditors[selectedPrimitive].Update();
                TransformPoints();
            }

            EditorGUI.BeginChangeCheck();
            primitiveEditors[selectedPrimitive].Draw();
            if (EditorGUI.EndChangeCheck()) TransformPoints();
        }

        private void PresetsGUI()
        {
            if (createPresetMode)
            {
                savePresetName = EditorGUILayout.TextField("Preset name", savePresetName);
                EditorGUILayout.LabelField("Description");
                savePresetDescription = EditorGUILayout.TextArea(savePresetDescription);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    var lower     = savePresetName.ToLower();
                    var noSlashes = lower.Replace('/', '_');
                    noSlashes = noSlashes.Replace('\\', '_');
                    var noSpaces = noSlashes.Replace(' ', '_');
                    var preset   = new SplinePreset(points, isClosed, splineType);
                    preset.name        = savePresetName;
                    preset.description = savePresetDescription;
                    preset.Save(noSpaces);
                    createPresetMode = false;
                    LoadPresets();
                    savePresetName = savePresetDescription = "";
                }

                if (GUILayout.Button("Cancel")) createPresetMode = false;
                EditorGUILayout.EndHorizontal();
                return;
            }

            if (GUILayout.Button("Create New")) createPresetMode = true;
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            selectedPreset = EditorGUILayout.Popup(selectedPreset, presetNames, GUILayout.MaxWidth(Screen.width / 3f));
            if (selectedPreset >= 0 && selectedPreset < presets.Length)
            {
                if (GUILayout.Button("Use")) LoadPreset(selectedPreset);
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                    if (EditorUtility.DisplayDialog("Delete Preset",
                                                    "This will permanently delete the preset file. Continue?", "Yes",
                                                    "No"))
                    {
                        SplinePreset.Delete(presets[selectedPreset].filename);
                        LoadPresets();
                        if (selectedPreset >= presets.Length) selectedPreset = presets.Length - 1;
                    }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void TransformPoints()
        {
            for (var i = 0; i < editor.points.Length; i++)
            {
                editor.points[i].position = dsEditor.spline.transform.TransformPoint(editor.points[i].position);
                editor.points[i].tangent  = dsEditor.spline.transform.TransformPoint(editor.points[i].tangent);
                editor.points[i].tangent2 = dsEditor.spline.transform.TransformPoint(editor.points[i].tangent2);
                editor.points[i].normal   = dsEditor.spline.transform.TransformDirection(editor.points[i].normal);
            }

            SetDirty();
        }

        private void LoadPrimitives()
        {
            RecordUndo("Spline Primitive");
            var types = typeof(PrimitiveEditor).GetAllDerivedClasses();
            primitiveEditors = new PrimitiveEditor[types.Count];
            var count = 0;
            primitiveNames = new string[types.Count];
            foreach (var t in types)
            {
                primitiveEditors[count] = (PrimitiveEditor) Activator.CreateInstance(t);
                primitiveNames[count]   = primitiveEditors[count].GetName();
                count++;
            }

            if (selectedPrimitive >= 0 && selectedPrimitive < primitiveEditors.Length)
            {
                ClearSelection();
                primitiveEditors[selectedPrimitive].Open(dsEditor);
                primitiveEditors[selectedPrimitive].Update();
                TransformPoints();
                FramePoints();
            }
        }

        private void LoadPresets()
        {
            ApplyDialog();
            RecordUndo("Spline Preset");
            presets     = SplinePreset.LoadAll();
            presetNames = new string[presets.Length];
            for (var i = 0; i < presets.Length; i++) presetNames[i] = presets[i].name;
            ClearSelection();
        }

        private void LoadPreset(int index)
        {
            if (index >= 0 && index < presets.Length)
            {
                points            = presets[index].points;
                editor.isClosed   = presets[index].isClosed;
                editor.splineType = presets[index].type;
                TransformPoints();
                FramePoints();
            }
        }

        private Vector3 GetOrigin(SplineComputer comp)
        {
            var avg                                       = Vector3.zero;
            var points                                    = comp.GetPoints(SplineComputer.Space.Local);
            for (var i = 0; i < comp.pointCount; i++) avg += points[i].position;
            if (points.Length > 0) avg                    /= points.Length;
            return avg;
        }
    }
}