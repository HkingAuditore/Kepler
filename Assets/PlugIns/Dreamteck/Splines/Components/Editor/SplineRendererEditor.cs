using UnityEditor;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplineRenderer), true)]
    [CanEditMultipleObjects]
    public class SplineRendererEditor : MeshGenEditor
    {
        protected override void BodyGUI()
        {
            showDoubleSided  = false;
            showFlipFaces    = false;
            showRotation     = false;
            showNormalMethod = false;


            serializedObject.Update();
            var slices              = serializedObject.FindProperty("_slices");
            var autoOrient          = serializedObject.FindProperty("autoOrient");
            var updateFrameInterval = serializedObject.FindProperty("updateFrameInterval");

            base.BodyGUI();
            EditorGUI.BeginChangeCheck();
            var user = (SplineRenderer) target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(slices);
            if (slices.intValue < 1) slices.intValue = 1;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Render", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(autoOrient);
            if (user.autoOrient)
            {
                EditorGUILayout.PropertyField(updateFrameInterval);
                if (updateFrameInterval.intValue < 0) updateFrameInterval.intValue = 0;
            }

            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

            UVControls(user);
        }
    }
}