using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(PolygonColliderGenerator))]
    [CanEditMultipleObjects]
    public class PolygonColliderGenEditor : SplineUserEditor
    {
        protected override void BodyGUI()
        {
            base.BodyGUI();
            var generator = (PolygonColliderGenerator) target;

            serializedObject.Update();
            var type       = serializedObject.FindProperty("_type");
            var size       = serializedObject.FindProperty("_size");
            var offset     = serializedObject.FindProperty("_offset");
            var updateRate = serializedObject.FindProperty("updateRate");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Polygon", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(type, new GUIContent("Type"));
            if (type.intValue == (int) PolygonColliderGenerator.Type.Path)
                EditorGUILayout.PropertyField(size, new GUIContent("Size"));
            EditorGUILayout.PropertyField(offset, new GUIContent("Offset"));
            EditorGUILayout.PropertyField(updateRate);
            if (updateRate.floatValue < 0f) updateRate.floatValue = 0f;
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}