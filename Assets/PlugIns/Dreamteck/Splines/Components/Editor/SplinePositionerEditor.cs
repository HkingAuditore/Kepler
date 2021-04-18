using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplinePositioner), true)]
    [CanEditMultipleObjects]
    public class SplinePositionerEditor : SplineTracerEditor
    {
        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Positioning", EditorStyles.boldLabel);

            serializedObject.Update();
            var mode = serializedObject.FindProperty("_mode");
            EditorGUI.BeginChangeCheck();
            var positioner = (SplinePositioner) target;
            EditorGUILayout.PropertyField(mode, new GUIContent("Mode"));
            if (positioner.mode == SplinePositioner.Mode.Distance)
            {
                positioner.position = EditorGUILayout.FloatField("Distance", (float) positioner.position);
            }
            else
            {
                var percent = serializedObject.FindProperty("_result").FindPropertyRelative("percent");

                EditorGUILayout.BeginHorizontal();
                var position = serializedObject.FindProperty("_position");
                var pos      = positioner.ClipPercent(percent.floatValue);
                EditorGUI.BeginChangeCheck();
                pos = EditorGUILayout.Slider("Percent", (float) pos, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) position.floatValue = (float) pos;
                if (GUILayout.Button("Set Distance", GUILayout.Width(85)))
                {
                    var w = EditorWindow.GetWindow<DistanceWindow>(true);
                    w.Init(OnSetDistance, positioner.CalculateLength());
                }

                EditorGUILayout.EndHorizontal();
            }

            var targetObject = serializedObject.FindProperty("_targetObject");
            EditorGUILayout.PropertyField(targetObject, new GUIContent("Target Object"));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            base.BodyGUI();
        }

        private void OnSetDistance(float distance)
        {
            for (var i = 0; i < targets.Length; i++)
            {
                var positioner = (SplinePositioner) targets[i];
                var travel     = positioner.Travel(0.0, distance);
                positioner.position = travel;
            }
        }
    }
}