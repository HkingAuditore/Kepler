using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(LengthCalculator), true)]
    [CanEditMultipleObjects]
    public class LengthCalculatorEditor : SplineUserEditor
    {
        public override void OnInspectorGUI()
        {
            showAveraging = false;
            base.OnInspectorGUI();
        }

        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Length Calculator", EditorStyles.boldLabel);
            base.BodyGUI();
            var calculator = (LengthCalculator) target;
            for (var i = 0; i < targets.Length; i++)
            {
                var lengthCalc = (LengthCalculator) targets[i];
                if (lengthCalc.spline == null) continue;
                EditorGUILayout.HelpBox(lengthCalc.spline.name + " Length: " + lengthCalc.length, MessageType.Info);
            }

            if (targets.Length > 1) return;
            var events = serializedObject.FindProperty("lengthEvents");

            EditorGUI.BeginChangeCheck();
            for (var i = 0; i < events.arraySize; i++)
            {
                var eventProperty = events.GetArrayElementAtIndex(i);
                var onChange      = eventProperty.FindPropertyRelative("onChange");
                var enabled       = eventProperty.FindPropertyRelative("enabled");
                var targetLength  = eventProperty.FindPropertyRelative("targetLength");
                var type          = eventProperty.FindPropertyRelative("type");

                EditorGUIUtility.labelWidth = 100;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(enabled, new GUIContent(""), GUILayout.Width(20));
                EditorGUILayout.PropertyField(targetLength);
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.PropertyField(type);
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    Undo.RecordObject(calculator, "Remove Length Event");
                    UnityEditor.ArrayUtility.RemoveAt(ref calculator.lengthEvents, i);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.PropertyField(onChange);
                EditorGUILayout.Space();
            }

            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Add Length Event"))
            {
                Undo.RecordObject(calculator, "Add Length Event");
                UnityEditor.ArrayUtility.Add(ref calculator.lengthEvents, new LengthCalculator.LengthEvent());
            }
        }
    }
}