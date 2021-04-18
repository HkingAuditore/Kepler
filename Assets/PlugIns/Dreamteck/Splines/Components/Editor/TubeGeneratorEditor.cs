using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(TubeGenerator))]
    [CanEditMultipleObjects]
    public class TubeGeneratorEditor : MeshGenEditor
    {
        protected override void BodyGUI()
        {
            base.BodyGUI();
            var tubeGenerator = (TubeGenerator) target;
            serializedObject.Update();
            var sides   = serializedObject.FindProperty("_sides");
            var capMode = serializedObject.FindProperty("_capMode");
            var revolve = serializedObject.FindProperty("_revolve");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sides,   new GUIContent("Sides"));
            EditorGUILayout.PropertyField(capMode, new GUIContent("Cap"));
            EditorGUILayout.PropertyField(revolve, new GUIContent("Revolve"));
            if (capMode.intValue == (int) TubeGenerator.CapMethod.Round)
            {
                var latitude = serializedObject.FindProperty("_roundCapLatitude");
                EditorGUILayout.PropertyField(latitude, new GUIContent("Cap Latitude"));
            }

            if (sides.intValue < 3) sides.intValue = 3;
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

            UVControls(tubeGenerator);
            var uvTwist = serializedObject.FindProperty("_uvTwist");
            EditorGUILayout.PropertyField(uvTwist, new GUIContent("UV Twist"));
            if (capMode.intValue != 0)
            {
                var capUVScale = serializedObject.FindProperty("_capUVScale");
                EditorGUILayout.PropertyField(capUVScale, new GUIContent("Cap UV Scale"));
            }
        }
    }
}