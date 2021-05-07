/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Inspector editor script for <c>AudioCapture</c> component.
  /// </summary>
  [CustomEditor(typeof(AudioCapture))]
  public class AudioCaptureEditor : UnityEditor.Editor
  {
    AudioCapture audioCapture;

    public void OnEnable()
    {
      audioCapture = (AudioCapture)target;
    }

    public override void OnInspectorGUI()
    {
      // Capture Control Section
      GUILayout.Label("Capture Control", EditorStyles.boldLabel);

      audioCapture.startOnAwake = EditorGUILayout.Toggle("Start On Awake", audioCapture.startOnAwake);
      if (audioCapture.startOnAwake)
      {
        audioCapture.captureTime = EditorGUILayout.FloatField("Capture Duration (Sec)", audioCapture.captureTime);
        audioCapture.quitAfterCapture = EditorGUILayout.Toggle("Quit After Capture", audioCapture.quitAfterCapture);
      }

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      audioCapture.saveFolder = EditorGUILayout.TextField("Save Folder", audioCapture.saveFolder);
      audioCapture.captureMicrophone = EditorGUILayout.Toggle("Capture Microphone", audioCapture.captureMicrophone);
      if (audioCapture.captureMicrophone)
      {
        audioCapture.deviceIndex = EditorGUILayout.IntField("Device Index", audioCapture.deviceIndex);
      }

      //// Tools Section
      //GUILayout.Label("Tools", EditorStyles.boldLabel);
      GUILayout.Space(10);

      if (GUILayout.Button("Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(audioCapture.saveFolder);
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