using System;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplineUser), true)]
    [CanEditMultipleObjects]
    public class SplineUserEditor : UnityEditor.Editor
    {
        private   int                 _editIndex = -1; //0 is reserved for editing clip values
        protected ColorModifierEditor colorModifierEditor;

        private bool doRebuild;

        protected GUIContent editButtonContent = new GUIContent("Edit", "Enable edit mode in scene view");
        protected GUIStyle   foldoutHeaderStyle;

        private SerializedProperty multithreadedProperty,
            updateMethodProperty,
            buildOnAwakeProperty,
            buildOnEnableProperty,
            autoUpdateProperty,
            loopSamplesProperty,
            clipFromProperty,
            clipToProperty;

        protected OffsetModifierEditor   offsetModifierEditor;
        protected RotationModifierEditor rotationModifierEditor;
        private   bool                   settingsFoldout;
        protected bool                   showAveraging      = true;
        protected bool                   showClip           = true;
        protected bool                   showMultithreading = true;
        protected bool                   showUpdateMethod   = true;
        protected SizeModifierEditor     sizeModifierEditor;
        protected SerializedProperty     spline;
        protected SplineUser[]           users = new SplineUser[0];

        public int editIndex
        {
            get => _editIndex;
            set
            {
                if (value == 0)
                {
                    Debug.LogError("Cannot set edit index to 0. 0 is reserved.");
                    return;
                }

                if (value < -1) value = -1;
                _editIndex = value;
            }
        }

        protected virtual void Awake()
        {
#if UNITY_2018_OR_NEWER
            foldoutHeader = EditorStyles.foldoutHeader;
#else
            foldoutHeaderStyle = EditorStyles.foldout;
#endif
        }

        protected virtual void OnEnable()
        {
            var user = (SplineUser) target;

            settingsFoldout = EditorPrefs.GetBool("Dreamteck.Splines.Editor.SplineUserEditor.settingsFoldout", false);
            rotationModifierEditor = new RotationModifierEditor(user, this, user.rotationModifier);
            offsetModifierEditor = new OffsetModifierEditor(user, this, user.offsetModifier);
            colorModifierEditor = new ColorModifierEditor(user, this, user.colorModifier);
            sizeModifierEditor = new SizeModifierEditor(user, this, user.sizeModifier);

            updateMethodProperty  = serializedObject.FindProperty("updateMethod");
            buildOnAwakeProperty  = serializedObject.FindProperty("buildOnAwake");
            buildOnEnableProperty = serializedObject.FindProperty("buildOnEnable");
            multithreadedProperty = serializedObject.FindProperty("multithreaded");
            autoUpdateProperty    = serializedObject.FindProperty("_autoUpdate");
            loopSamplesProperty   = serializedObject.FindProperty("_loopSamples");
            clipFromProperty      = serializedObject.FindProperty("_clipFrom");
            clipToProperty        = serializedObject.FindProperty("_clipTo");
            spline                = serializedObject.FindProperty("_spline");

            users = new SplineUser[targets.Length];
            for (var i = 0; i < users.Length; i++)
            {
                users[i] = (SplineUser) targets[i];
                if (users[i].isActiveAndEnabled) user.EditorAwake();
            }

            Undo.undoRedoPerformed += OnUndoRedo;
        }


        protected virtual void OnDisable()
        {
            EditorPrefs.SetBool("Dreamteck.Splines.Editor.SplineUserEditor.settingsFoldout", settingsFoldout);
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        protected virtual void OnDestroy()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                if (target == null)
                    OnDelete(); //The object or the component is being deleted
                else
                    DoRebuild();
            }

            SplineComputerEditor.hold = false;
        }

        protected virtual void OnSceneGUI()
        {
            if (doRebuild) DoRebuild();
            var user = (SplineUser) target;
            if (user == null) return;
            if (user.spline != null)
            {
                var rootComputer = user.GetComponent<SplineComputer>();
                var allComputers = user.spline.GetConnectedComputers();
                for (var i = 0; i < allComputers.Count; i++)
                {
                    if (allComputers[i] == rootComputer && _editIndex == -1) continue;
                    if (allComputers[i].alwaysDraw) continue;
                    DSSplineDrawer.DrawSplineComputer(allComputers[i], 0.0, 1.0, 0.4f);
                }

                DSSplineDrawer.DrawSplineComputer(user.spline);
            }

            if (_editIndex             == 0) SceneClipEdit();
            if (offsetModifierEditor   != null) offsetModifierEditor.DrawScene();
            if (rotationModifierEditor != null) rotationModifierEditor.DrawScene();
            if (colorModifierEditor    != null) colorModifierEditor.DrawScene();
            if (sizeModifierEditor     != null) sizeModifierEditor.DrawScene();
        }

        protected virtual void HeaderGUI()
        {
            var user = (SplineUser) target;
            Undo.RecordObject(user, "Inspector Change");
            var lastSpline = (SplineComputer) spline.objectReferenceValue;
            EditorGUILayout.PropertyField(spline);
            var newSpline = (SplineComputer) spline.objectReferenceValue;
            if (lastSpline != (SplineComputer) spline.objectReferenceValue)
            {
                for (var i = 0; i < users.Length; i++)
                {
                    if (lastSpline != null) lastSpline.Unsubscribe(users[i]);
                    if (newSpline  != null) newSpline.Subscribe(users[i]);
                }

                user.Rebuild();
            }


            if (user.spline == null)
                EditorGUILayout
                   .HelpBox("No SplineComputer is referenced. Link a SplineComputer to make this SplineUser work.",
                            MessageType.Error);

            settingsFoldout = EditorGUILayout.Foldout(settingsFoldout, "User Configuration", foldoutHeaderStyle);
            if (settingsFoldout)
            {
                EditorGUI.indentLevel++;
                if (showClip) InspectorClipEdit();
                if (showUpdateMethod) EditorGUILayout.PropertyField(updateMethodProperty);
                EditorGUILayout.PropertyField(autoUpdateProperty, new GUIContent("Auto Rebuild"));
                if (showMultithreading) EditorGUILayout.PropertyField(multithreadedProperty);
                EditorGUILayout.PropertyField(buildOnAwakeProperty);
                EditorGUILayout.PropertyField(buildOnEnableProperty);
                EditorGUI.indentLevel--;
            }
        }

        private void InspectorClipEdit()
        {
            var isClosed    = true;
            var loopSamples = true;
            for (var i = 0; i < users.Length; i++)
                if (users[i].spline == null) isClosed        = false;
                else if (!users[i].spline.isClosed) isClosed = false;
                else if (!users[i].loopSamples) loopSamples  = false;

            var clipFrom = clipFromProperty.floatValue;
            var clipTo   = clipToProperty.floatValue;

            if (isClosed && loopSamples)
            {
                EditorGUILayout.BeginHorizontal();
                if (EditButton(_editIndex == 0))
                {
                    if (_editIndex == 0) _editIndex = -1;
                    else _editIndex                 = 0;
                }

                EditorGUILayout.BeginVertical();
                clipFrom = EditorGUILayout.Slider("Clip From", clipFrom, 0f, 1f);
                clipTo   = EditorGUILayout.Slider("Clip To",   clipTo,   0f, 1f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (EditButton(_editIndex == 0))
                {
                    if (_editIndex == 0) _editIndex = -1;
                    else _editIndex                 = 0;
                }

                EditorGUIUtility.labelWidth = 80f;
                EditorGUILayout.MinMaxSlider(new GUIContent("Clip Range:"), ref clipFrom, ref clipTo, 0f, 1f);
                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(30));
                clipFrom = EditorGUILayout.FloatField(clipFrom);
                clipTo   = EditorGUILayout.FloatField(clipTo);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
            }

            clipFromProperty.floatValue = clipFrom;
            clipToProperty.floatValue   = clipTo;
            SplineComputerEditor.hold   = _editIndex >= 0;

            if (isClosed) EditorGUILayout.PropertyField(loopSamplesProperty, new GUIContent("Loop Samples"));
            if (!loopSamplesProperty.boolValue || !isClosed)
                if (clipFromProperty.floatValue > clipToProperty.floatValue)
                {
                    var temp = clipToProperty.floatValue;
                    clipToProperty.floatValue   = clipFromProperty.floatValue;
                    clipFromProperty.floatValue = temp;
                }
        }

        protected virtual void BodyGUI()
        {
            EditorGUILayout.Space();
        }

        protected virtual void FooterGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sample Modifiers", EditorStyles.boldLabel);
            if (users.Length == 1)
            {
                if (offsetModifierEditor   != null) offsetModifierEditor.DrawInspector();
                if (rotationModifierEditor != null) rotationModifierEditor.DrawInspector();
                if (colorModifierEditor    != null) colorModifierEditor.DrawInspector();
                if (sizeModifierEditor     != null) sizeModifierEditor.DrawInspector();
            }
            else
            {
                EditorGUILayout.LabelField("Modifiers not available when multiple Spline Users are selected.",
                                           EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();
        }

        private void SceneClipEdit()
        {
            if (users.Length > 1) return;
            var user = (SplineUser) target;
            if (user.spline == null) return;
            var col = user.spline.editorPathColor;
            Undo.RecordObject(user, "Edit Clip Range");
            var val = user.clipFrom;
            SplineComputerEditorHandles.Slider(user.spline, ref val, col, "Clip From",
                                               SplineComputerEditorHandles.SplineSliderGizmo.ForwardTriangle);
            if (val != user.clipFrom) user.clipFrom = val;
            val = user.clipTo;
            SplineComputerEditorHandles.Slider(user.spline, ref val, col, "Clip To",
                                               SplineComputerEditorHandles.SplineSliderGizmo.BackwardTriangle);
            if (val != user.clipTo) user.clipTo = val;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (doRebuild) DoRebuild();
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            HeaderGUI();
            if (EditorGUI.EndChangeCheck()) ApplyAndRebuild();

            EditorGUI.BeginChangeCheck();
            BodyGUI();
            if (EditorGUI.EndChangeCheck()) ApplyAndRebuild();

            EditorGUI.BeginChangeCheck();
            FooterGUI();
            if (EditorGUI.EndChangeCheck()) ApplyAndRebuild();
        }

        private void ApplyAndRebuild()
        {
            serializedObject.ApplyModifiedProperties();
            DoRebuild();
        }

        private void DoRebuild()
        {
            for (var i = 0; i < users.Length; i++)
                if (users[i] && users[i].isActiveAndEnabled)
                    try
                    {
                        users[i].Rebuild();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }

            doRebuild = false;
        }

        protected virtual void OnDelete()
        {
        }

        protected virtual void OnUndoRedo()
        {
            doRebuild = true;
        }

        public bool EditButton(bool selected)
        {
            var width = 40f;
            editButtonContent.image = ResourceUtility.EditorLoadTexture("Splines/Editor/Icons", "edit_cursor");
            if (editButtonContent.image != null)
            {
                editButtonContent.text = "";
                width                  = 25f;
            }

            SplineEditorGUI.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
            if (SplineEditorGUI.EditorLayoutSelectableButton(editButtonContent, true, selected, GUILayout.Width(width)))
            {
                SceneView.RepaintAll();
                return true;
            }

            return false;
        }
    }
}