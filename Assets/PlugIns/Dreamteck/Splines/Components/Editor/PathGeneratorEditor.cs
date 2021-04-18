using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(PathGenerator), true)]
    [CanEditMultipleObjects]
    public class PathGeneratorEditor : MeshGenEditor
    {
        protected override void BodyGUI()
        {
            base.BodyGUI();
            var pathGenerator = (PathGenerator) target;
            serializedObject.Update();
            var slices        = serializedObject.FindProperty("_slices");
            var shape         = serializedObject.FindProperty("_shape");
            var shapeExposure = serializedObject.FindProperty("_shapeExposure");
            var useShapeCurve = serializedObject.FindProperty("_useShapeCurve");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(slices,        new GUIContent("Slices"));
            EditorGUILayout.PropertyField(useShapeCurve, new GUIContent("Use Shape Curve"));
            if (useShapeCurve.boolValue)
            {
                if (shape.animationCurveValue == null || shape.animationCurveValue.keys.Length == 0)
                {
                    shape.animationCurveValue = new AnimationCurve();
                    shape.animationCurveValue.AddKey(new Keyframe(0, 0));
                    shape.animationCurveValue.AddKey(new Keyframe(1, 0));
                }

                if (slices.intValue == 1)
                    EditorGUILayout
                       .HelpBox("Slices are set to 1. The curve shape may not be approximated correctly. You can increase the slices in order to fix that.",
                                MessageType.Warning);
                EditorGUILayout.PropertyField(shape,         new GUIContent("Shape Curve"));
                EditorGUILayout.PropertyField(shapeExposure, new GUIContent("Shape Exposure"));
            }

            if (slices.intValue < 1) slices.intValue = 1;
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();


            UVControls(pathGenerator);
        }
    }
}