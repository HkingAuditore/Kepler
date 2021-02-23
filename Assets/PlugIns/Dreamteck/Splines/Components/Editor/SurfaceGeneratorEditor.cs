namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;

    [CustomEditor(typeof(SurfaceGenerator))]
    [CanEditMultipleObjects]
    public class SurfaceGeneratorEditor : MeshGenEditor
    {
        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            SurfaceGenerator user = (SurfaceGenerator)target;
            if (user.extrudeSpline != null)
            {
                DSSplineDrawer.DrawSplineComputer(user.extrudeSpline, 0.0, 1.0, 0.5f);
            }
        }
        
        protected override void BodyGUI()
        {
            showSize = false;
            showRotation = false;
            base.BodyGUI();
            SurfaceGenerator user = (SurfaceGenerator)target;
            serializedObject.Update();
            SerializedProperty expand = serializedObject.FindProperty("_expand");
            SerializedProperty extrude = serializedObject.FindProperty("_extrude");
            SerializedProperty extrudeSpline = serializedObject.FindProperty("_extrudeSpline");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(expand, new GUIContent("Expand"));
            if (extrudeSpline.objectReferenceValue == null) EditorGUILayout.PropertyField(extrude, new GUIContent("Extrude"));
            EditorGUILayout.PropertyField(extrudeSpline, new GUIContent("Extrude Path"));
            if (extrudeSpline.objectReferenceValue != null)
            {
                SerializedProperty extrudeClipFrom = serializedObject.FindProperty("_extrudeFrom");
                SerializedProperty extrudeClipTo = serializedObject.FindProperty("_extrudeTo");
                float clipFrom = extrudeClipFrom.floatValue;
                float clipTo = extrudeClipTo.floatValue;
                EditorGUILayout.MinMaxSlider(new GUIContent("Extrude Clip Range:"), ref clipFrom, ref clipTo, 0f, 1f);
                extrudeClipFrom.floatValue = clipFrom;
                extrudeClipTo.floatValue = clipTo;
            }
            bool change = false;
            if (EditorGUI.EndChangeCheck())
            {
                change = true;
                serializedObject.ApplyModifiedProperties();
            }

            UVControls(user);

            if (extrude.floatValue != 0f || extrudeSpline.objectReferenceValue != null)
            {
                EditorGUI.BeginChangeCheck();
                SerializedProperty sideUvOffset = serializedObject.FindProperty("_sideUvOffset");
                SerializedProperty sideUvScale = serializedObject.FindProperty("_sideUvScale");
                SerializedProperty uniformUvs = serializedObject.FindProperty("_uniformUvs");

                EditorGUILayout.PropertyField(sideUvOffset, new GUIContent("Side UV Offset"));
                EditorGUILayout.PropertyField(sideUvScale, new GUIContent("Side UV Scale"));
                EditorGUILayout.PropertyField(uniformUvs, new GUIContent("Unform UVs"));
                if (EditorGUI.EndChangeCheck())
                {
                    change = true;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (change)
            {
                for (int i = 0; i < users.Length; i++)
                {
                    users[i].Rebuild();
                }
            }
        }  
    }
}
