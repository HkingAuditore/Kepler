using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SurfaceGenerator))]
    [CanEditMultipleObjects]
    public class SurfaceGeneratorEditor : MeshGenEditor
    {
        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            var user = (SurfaceGenerator) target;
            if (user.extrudeSpline != null) DSSplineDrawer.DrawSplineComputer(user.extrudeSpline, 0.0, 1.0, 0.5f);
        }

        protected override void BodyGUI()
        {
            showSize     = false;
            showRotation = false;
            base.BodyGUI();
            var user = (SurfaceGenerator) target;
            serializedObject.Update();
            var expand        = serializedObject.FindProperty("_expand");
            var extrude       = serializedObject.FindProperty("_extrude");
            var extrudeSpline = serializedObject.FindProperty("_extrudeSpline");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(expand, new GUIContent("Expand"));
            if (extrudeSpline.objectReferenceValue == null)
                EditorGUILayout.PropertyField(extrude, new GUIContent("Extrude"));
            EditorGUILayout.PropertyField(extrudeSpline, new GUIContent("Extrude Path"));
            if (extrudeSpline.objectReferenceValue != null)
            {
                var extrudeClipFrom = serializedObject.FindProperty("_extrudeFrom");
                var extrudeClipTo   = serializedObject.FindProperty("_extrudeTo");
                var clipFrom        = extrudeClipFrom.floatValue;
                var clipTo          = extrudeClipTo.floatValue;
                EditorGUILayout.MinMaxSlider(new GUIContent("Extrude Clip Range:"), ref clipFrom, ref clipTo, 0f, 1f);
                extrudeClipFrom.floatValue = clipFrom;
                extrudeClipTo.floatValue   = clipTo;
            }

            var change = false;
            if (EditorGUI.EndChangeCheck())
            {
                change = true;
                serializedObject.ApplyModifiedProperties();
            }

            UVControls(user);

            if (extrude.floatValue != 0f || extrudeSpline.objectReferenceValue != null)
            {
                EditorGUI.BeginChangeCheck();
                var sideUvOffset = serializedObject.FindProperty("_sideUvOffset");
                var sideUvScale  = serializedObject.FindProperty("_sideUvScale");
                var uniformUvs   = serializedObject.FindProperty("_uniformUvs");

                EditorGUILayout.PropertyField(sideUvOffset, new GUIContent("Side UV Offset"));
                EditorGUILayout.PropertyField(sideUvScale,  new GUIContent("Side UV Scale"));
                EditorGUILayout.PropertyField(uniformUvs,   new GUIContent("Unform UVs"));
                if (EditorGUI.EndChangeCheck())
                {
                    change = true;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (change)
                for (var i = 0; i < users.Length; i++)
                    users[i].Rebuild();
        }
    }
}