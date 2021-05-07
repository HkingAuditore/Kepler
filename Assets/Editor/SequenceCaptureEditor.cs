/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Inspector editor script for <c>SequenceCapture</c> component.
  /// </summary>
  [CustomEditor(typeof(SequenceCapture))]
  public class SequenceCaptureEditor : UnityEditor.Editor
  {
    SequenceCapture sequenceCapture;
    SerializedProperty regularCamera;
    SerializedProperty stereoCamera;
    SerializedProperty inputTexture;
    SerializedProperty cursorImage;

    public void OnEnable()
    {
      sequenceCapture = (SequenceCapture)target;

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

      // Capture Control Section
      GUILayout.Label("Capture Controls", EditorStyles.boldLabel);

      sequenceCapture.startOnAwake = EditorGUILayout.Toggle("Start On Awake", sequenceCapture.startOnAwake);
      if (sequenceCapture.startOnAwake)
      {
        sequenceCapture.captureTime = EditorGUILayout.FloatField("Capture Duration (Sec)", sequenceCapture.captureTime);
        sequenceCapture.quitAfterCapture = EditorGUILayout.Toggle("Quit After Capture", sequenceCapture.quitAfterCapture);
      }

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      sequenceCapture.captureSource = (CaptureSource)EditorGUILayout.EnumPopup("Capture Source", sequenceCapture.captureSource);

      if (sequenceCapture.captureSource == CaptureSource.RENDERTEXTURE)
      {
        EditorGUILayout.PropertyField(inputTexture, new GUIContent("Render Texture"), true);
      }

      sequenceCapture.captureType = (CaptureType)EditorGUILayout.EnumPopup("Capture Type", sequenceCapture.captureType);
      sequenceCapture.saveFolder = EditorGUILayout.TextField("Save Folder", sequenceCapture.saveFolder);

      if (sequenceCapture.captureSource == CaptureSource.CAMERA)
      {
        sequenceCapture.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", sequenceCapture.captureMode);

        if (sequenceCapture.captureMode == CaptureMode._360)
        {
          sequenceCapture.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", sequenceCapture.projectionType);
        }

        if (sequenceCapture.captureMode == CaptureMode._360 &&
          sequenceCapture.projectionType == ProjectionType.CUBEMAP)
        {
          sequenceCapture.stereoMode = StereoMode.NONE;
        }
        else
        {
          sequenceCapture.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", sequenceCapture.stereoMode);
        }
        if (sequenceCapture.stereoMode != StereoMode.NONE)
        {
          sequenceCapture.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", sequenceCapture.interpupillaryDistance);
        }
      }
      else
      {
        sequenceCapture.captureMode = CaptureMode.REGULAR;
        sequenceCapture.projectionType = ProjectionType.NONE;
        sequenceCapture.stereoMode = StereoMode.NONE;
      }

      if (sequenceCapture.captureSource == CaptureSource.SCREEN)
      {
        sequenceCapture.captureCursor = EditorGUILayout.Toggle("Capture Cursor", sequenceCapture.captureCursor);
        if (sequenceCapture.captureCursor)
        {
          EditorGUILayout.PropertyField(cursorImage, new GUIContent("Cursor Image"), true);
        }
      }

      // Capture Options Section
      GUILayout.Label("Image Settings", EditorStyles.boldLabel);

      sequenceCapture.imageFormat = (ImageFormat)EditorGUILayout.EnumPopup("Image Format", sequenceCapture.imageFormat);
      if (sequenceCapture.imageFormat == ImageFormat.JPEG)
      {
        sequenceCapture.jpgQuality = EditorGUILayout.IntField("Encode Quality", sequenceCapture.jpgQuality);
      }

      if (sequenceCapture.captureSource == CaptureSource.CAMERA)
      {
        sequenceCapture.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", sequenceCapture.resolutionPreset);
        if (sequenceCapture.resolutionPreset == ResolutionPreset.CUSTOM)
        {
          sequenceCapture.frameWidth = EditorGUILayout.IntField("Frame Width", sequenceCapture.frameWidth);
          sequenceCapture.frameHeight = EditorGUILayout.IntField("Frame Height", sequenceCapture.frameHeight);
          //sequenceCapture.bitrate = EditorGUILayout.IntField("Bitrate (Kbps)", sequenceCapture.bitrate);
        }
      }

      sequenceCapture.frameRate = (System.Int16)EditorGUILayout.IntField("Frame Rate", sequenceCapture.frameRate);
      if (sequenceCapture.captureMode == CaptureMode._360)
      {
        sequenceCapture.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", sequenceCapture.cubemapFaceSize);
      }
      sequenceCapture.antiAliasing = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing", sequenceCapture.antiAliasing);

      if (sequenceCapture.captureSource == CaptureSource.CAMERA)
      {
        sequenceCapture.transparent = EditorGUILayout.Toggle("Transparent", sequenceCapture.transparent);
      }

      // Capture Options Section
      GUILayout.Label("Encoder Settings", EditorStyles.boldLabel);

      sequenceCapture.encodeFromImages = EditorGUILayout.Toggle("Encode Video From Images", sequenceCapture.encodeFromImages);
      if (sequenceCapture.encodeFromImages)
      {
        sequenceCapture.encoderPreset = (EncoderPreset)EditorGUILayout.EnumPopup("Encoder Preset", sequenceCapture.encoderPreset);
        sequenceCapture.cleanImages = EditorGUILayout.Toggle("Clean Images After Encode", sequenceCapture.cleanImages);
      }

      //// Tools Section
      //GUILayout.Label("Tools", EditorStyles.boldLabel);
      GUILayout.Space(10);

      if (GUILayout.Button("Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(sequenceCapture.saveFolder);
      }
      //if (GUILayout.Button("Convert Last Video to WMV"))
      //{
      //  Utils.ConvertVideoWmv(VideoSaveUtils.lastVideoFile);
      //}
      //if (GUILayout.Button("Convert Last Video to AVI"))
      //{
      //  Utils.ConvertVideoAvi(VideoSaveUtils.lastVideoFile);
      //}
      //if (GUILayout.Button("Convert Last Video to FLV"))
      //{
      //  Utils.ConvertVideoFlv(VideoSaveUtils.lastVideoFile);
      //}
      //if (GUILayout.Button("Convert Last Video to MKV"))
      //{
      //  Utils.ConvertVideoMkv(VideoSaveUtils.lastVideoFile);
      //}
      //if (GUILayout.Button("Convert Last Video to GIF"))
      //{
      //  Utils.ConvertVideoGif(VideoSaveUtils.lastVideoFile);
      //}
      //if (GUILayout.Button("Encode Last Video to 4K"))
      //{
      //  Utils.EncodeVideo4K(VideoSaveUtils.lastVideoFile);
      //}
      //if (GUILayout.Button("Encode Last Video to 8K"))
      //{
      //  Utils.EncodeVideo8K(VideoSaveUtils.lastVideoFile);
      //}

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