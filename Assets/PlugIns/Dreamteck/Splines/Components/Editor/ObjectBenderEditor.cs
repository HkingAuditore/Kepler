using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(ObjectBender), true)]
    [CanEditMultipleObjects]
    public class ObjectBenderEditor : SplineUserEditor
    {
        private          bool      generatedUvs;
        private          Vector2   scroll   = Vector2.zero;
        private readonly List<int> selected = new List<int>();

        protected override void Awake()
        {
            var bender = (ObjectBender) target;
            if (!Application.isPlaying) bender.UpdateReferences();
            base.Awake();
        }

        protected override void OnDestroy()
        {
            var user = (SplineUser) target;
            if (Application.isEditor && !Application.isPlaying)
            {
                if (user             == null) OnDelete(); //The object or the component is being deleted
                else if (user.spline != null)
                    if (!generatedUvs)
                        user.Rebuild();
            }

            SplineComputerEditor.hold = false;
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            var bender = (ObjectBender) target;
            if (selected.Count > 0)
            {
                Handles.BeginGUI();
                for (var i = 0; i < selected.Count; i++)
                {
                    var screenPosition =
                        HandleUtility.WorldToGUIPoint(bender.bendProperties[selected[i]].transform.transform.position);
                    DreamteckEditorGUI
                       .Label(new Rect(screenPosition.x - 120 + bender.bendProperties[selected[i]].transform.transform.name.Length * 4, screenPosition.y, 120, 25),
                              bender.bendProperties[selected[i]].transform.transform.name);
                }

                Handles.EndGUI();
            }

            for (var i = 0; i < bender.bendProperties.Length; i++)
                if (bender.bendProperties[i].bendSpline && bender.bendProperties[i].splineComputer != null)
                    DSSplineDrawer.DrawSplineComputer(bender.bendProperties[i].splineComputer, 0.0, 1.0, 0.2f);

            //Draw bounds
            if (bender.bend) return;
            var bound = bender.GetBounds();
            var a     = bender.transform.TransformPoint(bound.min);
            var b     = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.min.y, bound.min.z));
            var c     = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.min.y, bound.max.z));
            var d     = bender.transform.TransformPoint(new Vector3(bound.min.x, bound.min.y, bound.max.z));

            var e = bender.transform.TransformPoint(new Vector3(bound.min.x, bound.max.y, bound.min.z));
            var f = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.max.y, bound.min.z));
            var g = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.max.y, bound.max.z));
            var h = bender.transform.TransformPoint(new Vector3(bound.min.x, bound.max.y, bound.max.z));

            Handles.color = Color.gray;
            Handles.DrawLine(a, b);
            Handles.DrawLine(b, c);
            Handles.DrawLine(c, d);
            Handles.DrawLine(d, a);

            Handles.DrawLine(e, f);
            Handles.DrawLine(f, g);
            Handles.DrawLine(g, h);
            Handles.DrawLine(h, e);

            Handles.DrawLine(a, e);
            Handles.DrawLine(b, f);
            Handles.DrawLine(c, g);
            Handles.DrawLine(d, h);

            var r  = bender.transform.right;
            var fr = bender.transform.forward;

            switch (bender.axis)
            {
                case ObjectBender.Axis.Z:
                    Handles.color = Color.blue;
                    Handles.DrawLine(r + b, r + c);
                    break;
                case ObjectBender.Axis.X:
                    Handles.color = Color.red;
                    Handles.DrawLine(b - fr, a - fr);
                    break;
                case ObjectBender.Axis.Y:
                    Handles.color = Color.green;
                    Handles.DrawLine(b - fr + r, f - fr + r);
                    break;
            }
        }

        private void PropertyEditor(ObjectBender.BendProperty[] properties)
        {
            if (selected.Count == 0) return;
            int applyRotationCount = 0,
                applyScaleCount    = 0,
                enableCount        = 0,
                bendMeshCount      = 0,
                bendColliderCount  = 0,
                bendSplineCount    = 0;
            bool showMesh           = false, showCollider = false, showSpline = false;
            var  colliderUpdateRate = 0f;
            for (var i = 0; i < selected.Count; i++)
            {
                var property = properties[selected[i]];
                if (property.enabled) enableCount++;
                if (property.applyRotation) applyRotationCount++;
                if (property.applyScale) applyScaleCount++;
                if (property.bendMesh) bendMeshCount++;
                if (property.bendCollider) bendColliderCount++;
                if (property.bendSpline) bendSplineCount++;
                if (property.filter         != null) showMesh     = true;
                if (property.collider       != null) showCollider = true;
                if (property.splineComputer != null) showSpline   = true;
                colliderUpdateRate += property.colliderUpdateRate;
            }

            var enabled       = enableCount        == selected.Count;
            var applyRotation = applyRotationCount == selected.Count;
            var applyScale    = applyScaleCount    == selected.Count;
            var bendMesh      = bendMeshCount      == selected.Count;
            var bendCollider  = bendColliderCount  == selected.Count;
            var bendSpline    = bendSplineCount    == selected.Count;
            colliderUpdateRate /= selected.Count;
            bool lastEnabled      = enabled,
                lastApplyRotation = applyRotation,
                lastApplyScale    = applyScale,
                lastBendMesh      = bendMesh,
                lastBendCollider  = bendCollider,
                lastBendSpline    = bendSpline;
            var lastColliderUpdateRate = colliderUpdateRate;

            EditorGUIUtility.labelWidth = 90;
            EditorGUI.BeginChangeCheck();
            GUI.color = Color.white;
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(EditorGUIUtility.currentViewWidth - 50));
            EditorGUILayout.BeginHorizontal();
            enabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(20));
            if (selected.Count == 1) EditorGUILayout.LabelField(properties[selected[0]].transform.transform.name);
            else EditorGUILayout.LabelField("Multiple objects");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            applyRotation = EditorGUILayout.Toggle("Apply rotation", applyRotation);
            applyScale    = EditorGUILayout.Toggle("Apply scale",    applyScale);
            if (showSpline) bendSpline = EditorGUILayout.Toggle("Bend Spline", bendSpline);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (showMesh)
            {
                bendMesh = EditorGUILayout.Toggle("Bend Mesh", bendMesh);
                if (bendMesh)
                {
                    if (showCollider)
                    {
                        bendCollider = EditorGUILayout.Toggle("Bend Collider", bendCollider);
                        if (bendCollider)
                        {
                            EditorGUI.indentLevel++;
                            colliderUpdateRate = EditorGUILayout.FloatField("Update Rate", colliderUpdateRate);
                            EditorGUI.indentLevel--;
                        }
                    }
                    else
                    {
                        GUI.Label(new Rect(EditorGUIUtility.currentViewWidth / 2f - 25, 40, EditorGUIUtility.currentViewWidth / 2f - 30, 22),
                                  "No Mesh Colliders Available");
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No Meshes Available");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                for (var i = 0; i < selected.Count; i++)
                {
                    if (lastEnabled       != enabled) properties[selected[i]].enabled               = enabled;
                    if (lastApplyRotation != applyRotation) properties[selected[i]].applyRotation   = applyRotation;
                    if (lastApplyScale    != applyScale) properties[selected[i]].applyScale         = applyScale;
                    if (bendMesh          != lastBendMesh) properties[selected[i]].bendMesh         = bendMesh;
                    if (bendCollider      != lastBendCollider) properties[selected[i]].bendCollider = bendCollider;
                    if (bendSpline        != lastBendSpline) properties[selected[i]].bendSpline     = bendSpline;
                    if (lastColliderUpdateRate != colliderUpdateRate)
                        properties[selected[i]].colliderUpdateRate = colliderUpdateRate;
                }
        }

        private void GetChildCount(Transform parent, ref int count)
        {
            foreach (Transform child in parent)
            {
                count++;
                GetChildCount(child, ref count);
            }
        }

        public override void OnInspectorGUI()
        {
            showAveraging = false;
            base.OnInspectorGUI();
        }

        protected override void BodyGUI()
        {
            base.BodyGUI();
            var bender = (ObjectBender) target;

            serializedObject.Update();
            var axis        = serializedObject.FindProperty("_axis");
            var normalMode  = serializedObject.FindProperty("_normalMode");
            var forwardMode = serializedObject.FindProperty("_forwardMode");

            for (var i = 0; i < targets.Length; i++)
            {
                var objBender  = (ObjectBender) targets[i];
                var childCount = 0;
                GetChildCount(objBender.transform, ref childCount);
                if (objBender.bendProperties.Length - 1 != childCount && !Application.isPlaying)
                    objBender.UpdateReferences();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(axis,       new GUIContent("Axis"));
            EditorGUILayout.PropertyField(normalMode, new GUIContent("Up Vector"));

            if (normalMode.intValue == (int) ObjectBender.NormalMode.Custom)
            {
                var customNormal = serializedObject.FindProperty("_customNormal");
                EditorGUILayout.PropertyField(customNormal, new GUIContent("Custom Up"));
            }

            EditorGUILayout.PropertyField(forwardMode, new GUIContent("Forward Vector"));
            if (forwardMode.intValue == (int) ObjectBender.ForwardMode.Custom)
            {
                var customForward = serializedObject.FindProperty("_customForward");
                EditorGUILayout.PropertyField(customForward, new GUIContent("Custom Forward"));
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                for (var i = 0; i < targets.Length; i++)
                {
                    var objBender = (ObjectBender) targets[i];
                    objBender.Rebuild();
                }
            }

            if (targets.Length > 1)
            {
                EditorGUILayout.LabelField("Object properties unavailable when multiple benders are selected.",
                                           EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (!bender.bend)
            {
                float scrollHeight = Mathf.Min(bender.bendProperties.Length, 15) * 22;
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(scrollHeight + 5));

                for (var i = 0; i < bender.bendProperties.Length; i++)
                {
                    var isSelected = selected.Contains(i);
                    if (!bender.bendProperties[i].enabled)
                    {
                        GUI.color = Color.gray;
                        if (isSelected) GUI.color = Color.Lerp(Color.gray, SplinePrefs.highlightColor, 0.5f);
                    }
                    else
                    {
                        if (isSelected) GUI.color = SplinePrefs.highlightColor;
                        else GUI.color            = Color.white;
                    }

                    GUILayout.Box(bender.bendProperties[i].transform.transform.name, GUILayout.Height(18),
                                  GUILayout.Width(EditorGUIUtility.currentViewWidth - 60));
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) &&
                        Event.current.type == EventType.MouseDown)
                    {
                        if (Event.current.control)
                        {
                            if (!selected.Contains(i)) selected.Add(i);
                        }
                        else if (Event.current.shift && selected.Count > 0)
                        {
                            var from = selected[0];
                            selected.Clear();
                            if (from < i)
                                for (var n = from; n <= i; n++)
                                    selected.Add(n);
                            else
                                for (var n = from; n >= i; n--)
                                    selected.Add(n);
                        }
                        else
                        {
                            selected.Clear();
                            selected.Add(i);
                        }

                        Repaint();
                        SceneView.RepaintAll();
                    }

                    GUI.color = Color.white;
                }

                EditorGUILayout.EndScrollView();

                if (selected.Count > 0) PropertyEditor(bender.bendProperties);

                if (selected.Count > 0)
                    if (Event.current.type == EventType.KeyDown)
                    {
                        if (Event.current.keyCode == KeyCode.DownArrow)
                        {
                            if (selected.Count > 1)
                            {
                                var temp = selected[selected.Count - 1];
                                selected.Clear();
                                selected.Add(temp);
                            }

                            selected[0]++;
                            if (selected[0] >= bender.bendProperties.Length) selected[0] = 0;
                        }

                        if (Event.current.keyCode == KeyCode.UpArrow)
                        {
                            if (selected.Count > 1)
                            {
                                var temp = selected[0];
                                selected.Clear();
                                selected.Add(temp);
                            }

                            selected[0]--;
                            if (selected[0] < 0) selected[0] = bender.bendProperties.Length - 1;
                        }

                        Repaint();
                        SceneView.RepaintAll();
                        Event.current.Use();
                    }
            }

            var editModeText               = "Enter Edit Mode";
            if (!bender.bend) editModeText = "Bend";
            if (GUILayout.Button(editModeText))
            {
                if (bender.bend) bender.bend = false;
                else bender.bend             = true;
            }

            if (bender.bend && !generatedUvs)
                if (GUILayout.Button("Generate Lightmap UVS"))
                {
                    bender.EditorGenerateLightmapUVs();
                    generatedUvs = true;
                }
        }
    }
}