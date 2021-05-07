/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Inspector editor script for <c>VideoCapture</c> component.
  /// </summary>
  [CustomEditor(typeof(VideoCapture))]
  public class VideoCaptureEditor : UnityEditor.Editor
  {
    VideoCapture videoCapture;
    SerializedProperty regularCamera;
    SerializedProperty stereoCamera;
    SerializedProperty inputTexture;
    SerializedProperty cursorImage;
    SerializedProperty ffmpegEncoder;
    SerializedProperty nvidiaEncoder;
    SerializedProperty gpuEncoder;

    public void OnEnable()
    {
      videoCapture = (VideoCapture)target;

      regularCamera = serializedObject.FindProperty("regularCamera");
      stereoCamera = serializedObject.FindProperty("stereoCamera");

      inputTexture = serializedObject.FindProperty("inputTexture");
      cursorImage = serializedObject.FindProperty("cursorImage");

      ffmpegEncoder = serializedObject.FindProperty("ffmpegEncoder");
      nvidiaEncoder = serializedObject.FindProperty("nvidiaEncoder");
      gpuEncoder = serializedObject.FindProperty("gpuEncoder");
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

      videoCapture.startOnAwake = EditorGUILayout.Toggle("Start On Awake", videoCapture.startOnAwake);
      if (videoCapture.startOnAwake)
      {
        videoCapture.captureTime = EditorGUILayout.FloatField("Capture Duration (Sec)", videoCapture.captureTime);
        videoCapture.quitAfterCapture = EditorGUILayout.Toggle("Quit After Capture", videoCapture.quitAfterCapture);
      }

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      videoCapture.captureSource = (CaptureSource)EditorGUILayout.EnumPopup("Capture Source", videoCapture.captureSource);

      if (videoCapture.captureSource == CaptureSource.RENDERTEXTURE)
      {
        EditorGUILayout.PropertyField(inputTexture, new GUIContent("Render Texture"), true);
      }

      videoCapture.captureType = (CaptureType)EditorGUILayout.EnumPopup("Capture Type", videoCapture.captureType);
      if (videoCapture.captureType == CaptureType.VOD)
      {
        videoCapture.saveFolder = EditorGUILayout.TextField("Save Folder", videoCapture.saveFolder);
      }
      else if (videoCapture.captureType == CaptureType.LIVE)
      {
        videoCapture.liveStreamUrl = EditorGUILayout.TextField("Live Stream Url", videoCapture.liveStreamUrl);
      }
      if (videoCapture.captureSource == CaptureSource.CAMERA)
      {
        videoCapture.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", videoCapture.captureMode);

        if (videoCapture.captureMode == CaptureMode._360)
        {
          videoCapture.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", videoCapture.projectionType);
        }

        if (videoCapture.captureMode == CaptureMode._360 &&
          videoCapture.projectionType == ProjectionType.CUBEMAP)
        {
          videoCapture.stereoMode = StereoMode.NONE;
        }
        else
        {
          videoCapture.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", videoCapture.stereoMode);
        }
        if (videoCapture.stereoMode != StereoMode.NONE)
        {
          videoCapture.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", videoCapture.interpupillaryDistance);
        }
      }
      else
      {
        videoCapture.captureMode = CaptureMode.REGULAR;
        videoCapture.projectionType = ProjectionType.NONE;
        videoCapture.stereoMode = StereoMode.NONE;
      }

      if (videoCapture.captureSource == CaptureSource.SCREEN)
      {
        videoCapture.captureCursor = EditorGUILayout.Toggle("Capture Cursor", videoCapture.captureCursor);
        if (videoCapture.captureCursor)
        {
          EditorGUILayout.PropertyField(cursorImage, new GUIContent("Cursor Image"), true);
        }
      }

      videoCapture.captureAudio = EditorGUILayout.Toggle("Capture Audio", videoCapture.captureAudio);
      if (videoCapture.captureAudio)
      {
        videoCapture.captureMicrophone = EditorGUILayout.Toggle("Capture Microphone", videoCapture.captureMicrophone);
        if (videoCapture.captureMicrophone)
        {
          videoCapture.deviceIndex = EditorGUILayout.IntField("Device Index", videoCapture.deviceIndex);
        }
      }
      videoCapture.offlineRender = EditorGUILayout.Toggle("Offline Render", videoCapture.offlineRender);
      videoCapture.screenBlitter = EditorGUILayout.Toggle("Screen Blitter", videoCapture.screenBlitter);

      // Capture Options Section
      GUILayout.Label("Video Settings", EditorStyles.boldLabel);

      if (videoCapture.captureSource == CaptureSource.CAMERA)
      {
        videoCapture.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", videoCapture.resolutionPreset);
        if (videoCapture.resolutionPreset == ResolutionPreset.CUSTOM)
        {
          videoCapture.frameWidth = EditorGUILayout.IntField("Frame Width", videoCapture.frameWidth);
          videoCapture.frameHeight = EditorGUILayout.IntField("Frame Height", videoCapture.frameHeight);
          videoCapture.bitrate = EditorGUILayout.IntField("Bitrate (Kbps)", videoCapture.bitrate);
        }
      }
      else
      {
        videoCapture.bitrate = EditorGUILayout.IntField("Bitrate (Kbps)", videoCapture.bitrate);
      }

      videoCapture.frameRate = (System.Int16)EditorGUILayout.IntField("Frame Rate", videoCapture.frameRate);
      if (videoCapture.captureMode == CaptureMode._360)
      {
        videoCapture.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", videoCapture.cubemapFaceSize);
      }
      videoCapture.antiAliasing = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing", videoCapture.antiAliasing);

      if (videoCapture.captureSource == CaptureSource.CAMERA)
      {
        videoCapture.transparent = EditorGUILayout.Toggle("Transparent", videoCapture.transparent);
      }

      // Capture Options Section
      GUILayout.Label("Encoder Settings", EditorStyles.boldLabel);

      videoCapture.encoderPreset = (EncoderPreset)EditorGUILayout.EnumPopup("Encoder Preset", videoCapture.encoderPreset);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (!FreeTrial.Check())
      {
        videoCapture.gpuEncoding = EditorGUILayout.Toggle("GPU Encoding", videoCapture.gpuEncoding);
      }
      if (videoCapture.gpuEncoding)
      {
        videoCapture.legacyGpuEncoding = EditorGUILayout.Toggle("Legacy GPU Encoding", videoCapture.legacyGpuEncoding);
      }
#endif

      // Internal Settings
      GUILayout.Label("Internal Settings", EditorStyles.boldLabel);
      videoCapture.enableInternalSettings = EditorGUILayout.Toggle("Enable", videoCapture.enableInternalSettings);

      if (videoCapture.enableInternalSettings)
      {
        // Capture Encoder
        GUILayout.Label("Capture Encoders", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(ffmpegEncoder, new GUIContent("FFmpeg Encoder"), true);
        EditorGUILayout.PropertyField(nvidiaEncoder, new GUIContent("Nvidia Encoder"), true);
        EditorGUILayout.PropertyField(gpuEncoder, new GUIContent("GPU Encoder"), true);
      }

      //// Tools Section
      //GUILayout.Label("Tools", EditorStyles.boldLabel);
      GUILayout.Space(10);

      if (GUILayout.Button("Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(videoCapture.saveFolder);
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