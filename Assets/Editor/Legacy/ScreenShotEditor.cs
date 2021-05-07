/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Inspector editor script for <c>ScreenShot</c> component.
  /// </summary>
  [CustomEditor(typeof(ScreenShot))]
  public class ScreenShotEditor : UnityEditor.Editor
  {
    ScreenShot screenshot;
    SerializedProperty regularCamera;
    SerializedProperty stereoCamera;

    public void OnEnable()
    {
      screenshot = (ScreenShot)target;

      regularCamera = serializedObject.FindProperty("regularCamera");
      stereoCamera = serializedObject.FindProperty("stereoCamera");
    }

    public override void OnInspectorGUI()
    {
      // Capture Cameras
      GUILayout.Label("Capture Cameras", EditorStyles.boldLabel);

      EditorGUILayout.PropertyField(regularCamera, new GUIContent("Regular Camera"), true);
      EditorGUILayout.PropertyField(stereoCamera, new GUIContent("Stereo Camera"), true);

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      screenshot.saveFolder = EditorGUILayout.TextField("Save Folder", screenshot.saveFolder);

      screenshot.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", screenshot.captureMode);
      if (screenshot.captureMode == CaptureMode._360)
      {
        screenshot.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", screenshot.projectionType);
      }
      if (screenshot.captureMode == CaptureMode._360 &&
          screenshot.projectionType == ProjectionType.CUBEMAP)
      {
        screenshot.stereoMode = StereoMode.NONE;
      }
      else
      {
        screenshot.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", screenshot.stereoMode);
      }
      if (screenshot.stereoMode != StereoMode.NONE)
      {
        screenshot.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", screenshot.interpupillaryDistance);
      }

      // Capture Options Section
      GUILayout.Label("Screenshot Settings", EditorStyles.boldLabel);

      screenshot.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", screenshot.resolutionPreset);
      if (screenshot.resolutionPreset == ResolutionPreset.CUSTOM)
      {
        screenshot.frameWidth = EditorGUILayout.IntField("Frame Width", screenshot.frameWidth);
        screenshot.frameHeight = EditorGUILayout.IntField("Frame Height", screenshot.frameHeight);
      }
      if (screenshot.captureMode == CaptureMode._360)
      {
        screenshot.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", screenshot.cubemapFaceSize);
      }
      screenshot.antiAliasingSetting = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing Settings", screenshot.antiAliasingSetting);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

      // Capture Options Section
      GUILayout.Label("Encoder Settings", EditorStyles.boldLabel);
      if (!FreeTrial.Check())
      {
        screenshot.gpuEncoding = EditorGUILayout.Toggle("GPU Encoding", screenshot.gpuEncoding);
      }

#endif

      //// Tools Section
      //GUILayout.Label("Tools", EditorStyles.boldLabel);
      GUILayout.Space(10);

      if (GUILayout.Button("Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(screenshot.saveFolder);
      }

      if (GUI.changed)
      {
        EditorUtility.SetDirty(target);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
      }

      // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
      serializedObject.ApplyModifiedProperties();
    }
  }
}