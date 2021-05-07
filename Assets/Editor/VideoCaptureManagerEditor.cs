/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Inspector editor script for <c>VideoCaptureManager</c> component.
  /// </summary>
  [CustomEditor(typeof(VideoCaptureManager))]
  public class VideoCaptureManagerEditor : UnityEditor.Editor
  {
    VideoCaptureManager manager;
    SerializedProperty videoCaptures;

    public void OnEnable()
    {
      manager = (VideoCaptureManager)target;
      videoCaptures = serializedObject.FindProperty("videoCaptures");
    }

    public override void OnInspectorGUI()
    {

      // Capture Control Section
      GUILayout.Label("Capture Control", EditorStyles.boldLabel);

      manager.startOnAwake = EditorGUILayout.Toggle("Start On Awake", manager.startOnAwake);

      if (manager.startOnAwake)
      {
        manager.captureTime = EditorGUILayout.FloatField("Capture Duration (Sec)", manager.captureTime);
        manager.quitAfterCapture = EditorGUILayout.Toggle("Quit After Capture", manager.quitAfterCapture);
      }

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      manager.saveFolder = EditorGUILayout.TextField("Save Folder", manager.saveFolder);
      manager.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", manager.captureMode);
      if (manager.captureMode == CaptureMode._360)
      {
        manager.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", manager.projectionType);
      }
      if (manager.captureMode == CaptureMode._360 &&
          manager.projectionType == ProjectionType.CUBEMAP)
      {
        manager.stereoMode = StereoMode.NONE;
      }
      else
      {
        manager.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", manager.stereoMode);
      }
      if (manager.stereoMode != StereoMode.NONE)
      {
        manager.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", manager.interpupillaryDistance);
      }
      manager.captureAudio = EditorGUILayout.Toggle("Capture Audio", manager.captureAudio);
      if (manager.captureAudio)
      {
        manager.captureMicrophone = EditorGUILayout.Toggle("Capture Microphone", manager.captureMicrophone);
        if (manager.captureMicrophone)
        {
          manager.deviceIndex = EditorGUILayout.IntField("Device Index", manager.deviceIndex);
        }
      }
      manager.offlineRender = EditorGUILayout.Toggle("Offline Render", manager.offlineRender);

      // Capture Options Section
      GUILayout.Label("Video Settings", EditorStyles.boldLabel);

      manager.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", manager.resolutionPreset);
      if (manager.resolutionPreset == ResolutionPreset.CUSTOM)
      {
        manager.frameWidth = EditorGUILayout.IntField("Frame Width", manager.frameWidth);
        manager.frameHeight = EditorGUILayout.IntField("Frame Height", manager.frameHeight);
        manager.bitrate = EditorGUILayout.IntField("Bitrate (Kbps)", manager.bitrate);
      }
      manager.frameRate = (System.Int16)EditorGUILayout.IntField("Frame Rate", manager.frameRate);
      if (manager.captureMode == CaptureMode._360)
      {
        manager.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", manager.cubemapFaceSize);
      }
      manager.antiAliasing = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing", manager.antiAliasing);

      // Capture Options Section
      GUILayout.Label("Encoder Settings", EditorStyles.boldLabel);

      if (!manager.gpuEncoding)
      {
        manager.encoderPreset = (EncoderPreset)EditorGUILayout.EnumPopup("Encoder Preset", manager.encoderPreset);
      }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (!FreeTrial.Check())
      {
        manager.gpuEncoding = EditorGUILayout.Toggle("GPU Encoding", manager.gpuEncoding);
      }
#endif

      serializedObject.Update();
      EditorGUILayout.PropertyField(videoCaptures, new GUIContent("Video Captures"), true);
      // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
      serializedObject.ApplyModifiedProperties();

      //// Tools Section
      //GUILayout.Label("Tools", EditorStyles.boldLabel);
      GUILayout.Space(10);

      if (GUILayout.Button("Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(manager.saveFolder);
      }

      GUILayout.Space(10);

      if (GUI.changed)
      {
        EditorUtility.SetDirty(target);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
      }
    }
  }
}