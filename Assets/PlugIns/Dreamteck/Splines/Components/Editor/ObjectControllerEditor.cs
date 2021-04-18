using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(ObjectController))]
    [CanEditMultipleObjects]
    public class ObjectControllerEditor : SplineUserEditor
    {
        protected override void BodyGUI()
        {
            base.BodyGUI();
            var user = (ObjectController) target;
            serializedObject.Update();
            var objectMethod                  = serializedObject.FindProperty("_objectMethod");
            var retainPrefabInstancesInEditor = serializedObject.FindProperty("_retainPrefabInstancesInEditor");
            var spawnCount                    = serializedObject.FindProperty("_spawnCount");
            var delayedSpawn                  = serializedObject.FindProperty("delayedSpawn");
            var spawnDelay                    = serializedObject.FindProperty("spawnDelay");
            var iteration                     = serializedObject.FindProperty("_iteration");
            var applyRotation                 = serializedObject.FindProperty("_applyRotation");
            var minRotation                   = serializedObject.FindProperty("_minRotation");
            var maxRotation                   = serializedObject.FindProperty("_maxRotation");
            var applyScale                    = serializedObject.FindProperty("_applyScale");
            var minScaleMultiplier            = serializedObject.FindProperty("_minScaleMultiplier");
            var maxScaleMultiplier            = serializedObject.FindProperty("_maxScaleMultiplier");
            var uniformScaleLerp              = serializedObject.FindProperty("_uniformScaleLerp");
            var objectPositioning             = serializedObject.FindProperty("_objectPositioning");
            var evaluateOffset                = serializedObject.FindProperty("_evaluateOffset");
            var offsetUseWorldCoords          = serializedObject.FindProperty("_offsetUseWorldCoords");
            var minOffset                     = serializedObject.FindProperty("_minOffset");
            var maxOffset                     = serializedObject.FindProperty("_maxOffset");
            var shellOffset                   = serializedObject.FindProperty("_shellOffset");
            var rotateByOffset                = serializedObject.FindProperty("_rotateByOffset");
            var randomSeed                    = serializedObject.FindProperty("_randomSeed");


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(objectMethod, new GUIContent("Object Method"));
            if (objectMethod.intValue == (int) ObjectController.ObjectMethod.Instantiate)
                EditorGUILayout.PropertyField(retainPrefabInstancesInEditor, new GUIContent("Retain Prefab Instances"));
            if (objectMethod.intValue == (int) ObjectController.ObjectMethod.Instantiate)
            {
                var objectsChanged = false;
                var hasObj         = false;
                if (users.Length > 1)
                {
                    EditorGUILayout.HelpBox("Editing unavailable when multiple objects are selected", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Instantiate Objects", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical();

                    for (var i = 0; i < user.objects.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        user.objects[i] =
                            (GameObject) EditorGUILayout.ObjectField(user.objects[i], typeof(GameObject), true);
                        if (GUILayout.Button("x", GUILayout.Width(20)))
                        {
                            var newObjects = new GameObject[user.objects.Length - 1];
                            for (var n = 0; n < user.objects.Length; n++)
                            {
                                if (n      < i) newObjects[n] = user.objects[n];
                                else if (n == i) continue;
                                else newObjects[n - 1] = user.objects[n];
                                objectsChanged = true;
                            }

                            user.objects = newObjects;
                        }

                        if (i > 0)
                            if (GUILayout.Button("▲", GUILayout.Width(20)))
                            {
                                var temp = user.objects[i - 1];
                                user.objects[i - 1] = user.objects[i];
                                user.objects[i]     = temp;
                                objectsChanged      = true;
                            }

                        if (i < user.objects.Length - 1)
                            if (GUILayout.Button("▼", GUILayout.Width(20)))
                            {
                                var temp = user.objects[i + 1];
                                user.objects[i + 1] = user.objects[i];
                                user.objects[i]     = temp;
                                objectsChanged      = true;
                            }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    GameObject newObj = null;
                    newObj = (GameObject) EditorGUILayout.ObjectField("Add Object", newObj, typeof(GameObject), true);
                    if (newObj != null)
                    {
                        var newObjects = new GameObject[user.objects.Length + 1];
                        user.objects.CopyTo(newObjects, 0);
                        newObjects[newObjects.Length - 1] = newObj;
                        user.objects                      = newObjects;
                        objectsChanged                    = true;
                    }

                    for (var i = 0; i < user.objects.Length; i++)
                        if (user.objects[i] != null)
                        {
                            hasObj = true;
                            break;
                        }
                }

                var lastSpawnCount = spawnCount.intValue;
                if (hasObj) EditorGUILayout.PropertyField(spawnCount, new GUIContent("Spawn Count"));
                else spawnCount.intValue = 0;
                if (lastSpawnCount != spawnCount.intValue) objectsChanged = true;
                EditorGUILayout.PropertyField(delayedSpawn, new GUIContent("Delayed Spawn"));
                if (delayedSpawn.boolValue) EditorGUILayout.PropertyField(spawnDelay, new GUIContent("Spawn Delay"));

                var lastIteration = iteration.intValue;
                EditorGUILayout.PropertyField(iteration, new GUIContent("Iteration"));
                if (lastIteration != iteration.intValue) objectsChanged = true;

                if (objectsChanged)
                {
                    serializedObject.ApplyModifiedProperties();
                    user.Clear();
                    user.Spawn();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applyRotation, new GUIContent("Apply Rotation"));
            if (user.applyRotation)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(minRotation, new GUIContent("Min. Rotation Offset"));
                EditorGUILayout.PropertyField(maxRotation, new GUIContent("Max. Rotation Offset"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(applyScale, new GUIContent("Apply Scale"));
            if (user.applyScale)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(minScaleMultiplier, new GUIContent("Min. Scale Multiplier"));
                EditorGUILayout.PropertyField(maxScaleMultiplier, new GUIContent("Max. Scale Multiplier"));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(uniformScaleLerp, new GUIContent("Uniform Lerp"));
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(objectPositioning, new GUIContent("Object Positioning"));
            EditorGUILayout.PropertyField(evaluateOffset,    new GUIContent("Evaluate Offset"));


            if (offsetUseWorldCoords.boolValue)
            {
                minOffset.vector3Value = EditorGUILayout.Vector3Field("Min. Offset", minOffset.vector3Value);
                maxOffset.vector3Value = EditorGUILayout.Vector3Field("Max. Offset", maxOffset.vector3Value);
            }
            else
            {
                minOffset.vector3Value = EditorGUILayout.Vector2Field("Min. Offset", minOffset.vector3Value);
                maxOffset.vector3Value = EditorGUILayout.Vector2Field("Max. Offset", maxOffset.vector3Value);
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(offsetUseWorldCoords, new GUIContent("Use World Coords."));
            if (minOffset.vector3Value != maxOffset.vector3Value)
                EditorGUILayout.PropertyField(shellOffset, new GUIContent("Shell"));
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(rotateByOffset, new GUIContent("Rotate by Offset"));
            EditorGUILayout.PropertyField(randomSeed,     new GUIContent("Random Seed"));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}