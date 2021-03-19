using UnityEngine;
using UnityEditor;

namespace FlatKit {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LinearMotion))]
    public class LinearMotionEditor : Editor {
        private SerializedProperty _translationMode;
        private SerializedProperty _translationVector;
        private SerializedProperty _translationSpeed;
        private SerializedProperty _translationAcceleration;

        private SerializedProperty _rotationMode;
        private SerializedProperty _rotationAxis;
        private SerializedProperty _rotationSpeed;
        private SerializedProperty _rotationAcceleration;

        private SerializedProperty _useLocalCoordinate;

        private static readonly GUIContent TextRotation = new GUIContent("Rotation");
        private static readonly GUIContent TextAcceleration = new GUIContent("Acceleration");
        private static readonly GUIContent TextTranslation = new GUIContent("Translation");
        private static readonly GUIContent TextSpeed = new GUIContent("Speed");
        private static readonly GUIContent TextVector = new GUIContent("Vector");
        private static readonly GUIContent TextLocalCoordinate = new GUIContent("Local Coordinate");

        void OnEnable() {
            _translationMode = serializedObject.FindProperty("translationMode");
            _translationVector = serializedObject.FindProperty("translationVector");
            _translationSpeed = serializedObject.FindProperty("translationSpeed");
            _translationAcceleration = serializedObject.FindProperty("translationAcceleration");

            _rotationMode = serializedObject.FindProperty("rotationMode");
            _rotationAxis = serializedObject.FindProperty("rotationAxis");
            _rotationSpeed = serializedObject.FindProperty("rotationSpeed");
            _rotationAcceleration = serializedObject.FindProperty("rotationAcceleration");

            _useLocalCoordinate = serializedObject.FindProperty("useLocalCoordinate");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_translationMode, TextTranslation);

            EditorGUI.indentLevel++;

            if (_translationMode.hasMultipleDifferentValues ||
                _translationMode.enumValueIndex == (int) LinearMotion.TranslationMode.Vector) {
                EditorGUILayout.PropertyField(_translationVector, TextVector);
            }

            if (_translationMode.hasMultipleDifferentValues ||
                _translationMode.enumValueIndex != 0) {
                EditorGUILayout.PropertyField(_translationSpeed, TextSpeed);
                EditorGUILayout.PropertyField(_translationAcceleration, TextAcceleration);
            }
            
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(_rotationMode, TextRotation);

            EditorGUI.indentLevel++;

            if (_rotationMode.hasMultipleDifferentValues ||
                _rotationMode.enumValueIndex == (int) LinearMotion.RotationMode.Vector) {
                EditorGUILayout.PropertyField(_rotationAxis, TextVector);
            }

            if (_rotationMode.hasMultipleDifferentValues ||
                _rotationMode.enumValueIndex != 0) {
                EditorGUILayout.PropertyField(_rotationSpeed, TextSpeed);
                EditorGUILayout.PropertyField(_rotationAcceleration, TextAcceleration);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(_useLocalCoordinate, TextLocalCoordinate);

            serializedObject.ApplyModifiedProperties();
        }
    }
}