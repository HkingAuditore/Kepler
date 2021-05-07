/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Inspector editor script for <c>ImageCapture</c> component.
  /// </summary>
  [CustomEditor(typeof(ImageCapture))]
  public class ImageCaptureEditor : UnityEditor.Editor
  {
    ImageCapture imageCapture;
    SerializedProperty regularCamera;
    SerializedProperty stereoCamera;
    SerializedProperty inputTexture;
    SerializedProperty cursorImage;

    public void OnEnable()
    {
      imageCapture = (ImageCapture)target;

      regularCamera = serializedObject.FindProperty("regularCamera");
      stereoCamera = serializedObject.FindProperty("stereoCamera");
      inputTexture = serializedObject.FindProperty("inputTexture");
      cursorImage = serializedObject.FindProperty("cursorImage");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      // Capture Cameras
      GUILayout.Label("Capture Cameras", EditorStyles.boldLabel);

      EditorGUILayout.PropertyField(regularCamera, new GUIContent("Regular Camera"), true);
      EditorGUILayout.PropertyField(stereoCamera, new GUIContent("Stereo Camera"), true);

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      imageCapture.captureSource = (CaptureSource)EditorGUILayout.EnumPopup("Capture Source", imageCapture.captureSource);

      if (imageCapture.captureSource == CaptureSource.RENDERTEXTURE)
      {
        EditorGUILayout.PropertyField(inputTexture, new GUIContent("Render Texture"), true);
      }

      imageCapture.saveFolder = EditorGUILayout.TextField("Save Folder", imageCapture.saveFolder);

      if (imageCapture.captureSource == CaptureSource.CAMERA)
      {
        imageCapture.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", imageCapture.captureMode);

        if (imageCapture.captureMode == CaptureMode._360)
        {
          imageCapture.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", imageCapture.projectionType);
        }

        if (imageCapture.captureMode == CaptureMode._360 &&
          imageCapture.projectionType == ProjectionType.CUBEMAP)
        {
          imageCapture.stereoMode = StereoMode.NONE;
        }
        else
        {
          imageCapture.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", imageCapture.stereoMode);
        }
        if (imageCapture.stereoMode != StereoMode.NONE)
        {
          imageCapture.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", imageCapture.interpupillaryDistance);
        }
      }
      else
      {
        imageCapture.captureMode = CaptureMode.REGULAR;
        imageCapture.projectionType = ProjectionType.NONE;
        imageCapture.stereoMode = StereoMode.NONE;
      }

      if (imageCapture.captureSource == CaptureSource.SCREEN)
      {
        imageCapture.captureCursor = EditorGUILayout.Toggle("Capture Cursor", imageCapture.captureCursor);
        if (imageCapture.captureCursor)
        {
          EditorGUILayout.PropertyField(cursorImage, new GUIContent("Cursor Image"), true);
        }
      }

      // Capture Options Section
      GUILayout.Label("Image Settings", EditorStyles.boldLabel);

      imageCapture.imageFormat = (ImageFormat)EditorGUILayout.EnumPopup("Image Format", imageCapture.imageFormat);
      if (imageCapture.imageFormat == ImageFormat.JPEG)
      {
        imageCapture.jpgQuality = EditorGUILayout.IntField("Encode Quality", imageCapture.jpgQuality);
      }

      if (imageCapture.captureSource == CaptureSource.CAMERA)
      {
        imageCapture.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", imageCapture.resolutionPreset);
        if (imageCapture.resolutionPreset == ResolutionPreset.CUSTOM)
        {
          imageCapture.frameWidth = EditorGUILayout.IntField("Frame Width", imageCapture.frameWidth);
          imageCapture.frameHeight = EditorGUILayout.IntField("Frame Height", imageCapture.frameHeight);
        }
      }

      if (imageCapture.captureMode == CaptureMode._360)
      {
        imageCapture.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", imageCapture.cubemapFaceSize);
      }
      imageCapture.antiAliasing = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing", imageCapture.antiAliasing);

      if (imageCapture.captureSource == CaptureSource.CAMERA)
      {
        imageCapture.transparent = EditorGUILayout.Toggle("Transparent", imageCapture.transparent);
      }

      //// Tools Section
      //GUILayout.Label("Tools", EditorStyles.boldLabel);
      GUILayout.Space(10);

      if (GUILayout.Button("Capture"))
      {
        // Call start capture image
        imageCapture.StartCapture();
      }

      if (GUILayout.Button("Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(imageCapture.saveFolder);
      }

      GUILayout.Space(10);

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