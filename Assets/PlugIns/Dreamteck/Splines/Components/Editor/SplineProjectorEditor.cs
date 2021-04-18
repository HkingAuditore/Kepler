using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplineProjector), true)]
    [CanEditMultipleObjects]
    public class SplineProjectorEditor : SplineTracerEditor
    {
        private bool info;

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            for (var i = 0; i < users.Length; i++)
            {
                var user = (SplineProjector) users[i];
                if (user.spline == null) return;
                if (!user.autoProject) return;
                DrawResult(user.result);
            }
        }

        public override void OnInspectorGUI()
        {
            var user = (SplineProjector) target;
            if (user.mode == SplineProjector.Mode.Accurate)
                showAveraging = false;
            else
                showAveraging = true;
            base.OnInspectorGUI();
        }

        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Projector", EditorStyles.boldLabel);

            serializedObject.Update();
            var mode          = serializedObject.FindProperty("_mode");
            var projectTarget = serializedObject.FindProperty("_projectTarget");
            var targetObject  = serializedObject.FindProperty("_targetObject");
            var autoProject   = serializedObject.FindProperty("_autoProject");


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(mode, new GUIContent("Mode"));
            if (mode.intValue == (int) SplineProjector.Mode.Accurate)
            {
                var subdivide = serializedObject.FindProperty("_subdivide");
                EditorGUILayout.PropertyField(subdivide, new GUIContent("Subdivide"));
            }

            EditorGUILayout.PropertyField(projectTarget, new GUIContent("Project Target"));
            EditorGUILayout.PropertyField(targetObject,  new GUIContent("Apply Target"));

            GUI.color = Color.white;
            EditorGUILayout.PropertyField(autoProject, new GUIContent("Auto Project"));

            info = EditorGUILayout.Foldout(info, "Info");
            var percent = serializedObject.FindProperty("_result").FindPropertyRelative("percent");
            if (info) EditorGUILayout.HelpBox("Projection percent: " + percent.floatValue, MessageType.Info);

            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            base.BodyGUI();
        }
    }
}