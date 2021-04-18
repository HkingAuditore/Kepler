using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplineFollower), true)]
    [CanEditMultipleObjects]
    public class SplineFollowerEditor : SplineTracerEditor
    {
        protected        SplineFollower[]            followers = new SplineFollower[0];
        private readonly SplineSample                result    = new SplineSample();
        protected        SerializedObject            serializedFollowers;
        protected        FollowerSpeedModifierEditor speedModifierEditor;

        protected override void OnEnable()
        {
            base.OnEnable();
            followers = new SplineFollower[users.Length];
            for (var i = 0; i < followers.Length; i++) followers[i] = (SplineFollower) users[i];

            if (followers.Length == 1)
                speedModifierEditor = new FollowerSpeedModifierEditor(followers[0], this, followers[0].speedModifier);
        }


        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            var user = (SplineFollower) target;
            if (user == null) return;
            if (Application.isPlaying)
            {
                if (!user.follow) DrawResult(user.modifiedResult);
                return;
            }

            if (user.spline == null) return;
            if (user.autoStartPosition)
            {
                user.spline.Project(result, user.transform.position, user.clipFrom, user.clipTo);
                DrawResult(result);
            }
            else if (!user.follow)
            {
                DrawResult(user.result);
            }

            if (followers.Length == 1) speedModifierEditor.DrawScene();
        }

        private void OnSetDistance(float distance)
        {
            for (var i = 0; i < targets.Length; i++)
            {
                var follower = (SplineFollower) targets[i];
                var travel   = follower.Travel(0.0, distance);
                follower.startPosition = travel;
            }
        }

        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Following", EditorStyles.boldLabel);
            var follower = (SplineFollower) target;

            serializedFollowers = new SerializedObject(followers);
            var followMode                     = serializedObject.FindProperty("followMode");
            var preserveUniformSpeedWithOffset = serializedObject.FindProperty("preserveUniformSpeedWithOffset");
            var wrapMode                       = serializedObject.FindProperty("wrapMode");
            var startPosition                  = serializedObject.FindProperty("_startPosition");
            var autoStartPosition              = serializedObject.FindProperty("autoStartPosition");
            var follow                         = serializedObject.FindProperty("follow");
            var unityOnEndReached              = serializedObject.FindProperty("_unityOnEndReached");
            var unityOnBeginningReached        = serializedObject.FindProperty("_unityOnBeginningReached");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(follow);
            if (follow.boolValue)
            {
                EditorGUILayout.PropertyField(followMode);
                if (followMode.intValue == (int) SplineFollower.FollowMode.Uniform)
                {
                    var followSpeed     = serializedObject.FindProperty("_followSpeed");
                    var motion          = serializedObject.FindProperty("_motion");
                    var motionHasOffset = motion.FindPropertyRelative("_hasOffset");

                    EditorGUILayout.PropertyField(followSpeed, new GUIContent("Follow Speed"));
                    if (followSpeed.floatValue < 0f) followSpeed.floatValue = 0f;
                    if (motionHasOffset.boolValue)
                        EditorGUILayout.PropertyField(preserveUniformSpeedWithOffset,
                                                      new GUIContent("Preserve Uniform Speed With Offset"));
                    if (followers.Length == 1) speedModifierEditor.DrawInspector();
                }
                else
                {
                    follower.followDuration = EditorGUILayout.FloatField("Follow duration", follower.followDuration);
                }
            }


            EditorGUILayout.PropertyField(wrapMode);


            if (follower.motion.applyRotation)
                follower.applyDirectionRotation =
                    EditorGUILayout.Toggle("Face Direction", follower.applyDirectionRotation);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Start Position", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(autoStartPosition, new GUIContent("Project"));
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100f;
            if (!follower.autoStartPosition && !Application.isPlaying)
            {
                EditorGUILayout.PropertyField(startPosition, new GUIContent("Start Position"));
                if (GUILayout.Button("Set Distance", GUILayout.Width(85)))
                {
                    var w = EditorWindow.GetWindow<DistanceWindow>(true);
                    w.Init(OnSetDistance, follower.CalculateLength());
                }
            }
            else
            {
                EditorGUILayout.LabelField("Start position", GUILayout.Width(EditorGUIUtility.labelWidth));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(unityOnBeginningReached);
            EditorGUILayout.PropertyField(unityOnEndReached);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                    for (var i = 0; i < followers.Length; i++)
                        if (followers[i].spline.sampleCount > 0)
                            if (!followers[i].autoStartPosition)
                            {
                                followers[i].SetPercent(startPosition.floatValue);
                                if (!followers[i].follow) SceneView.RepaintAll();
                            }
            }

            base.BodyGUI();
        }
    }
}