/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>VideoCaptureManager</c> script helps you to capture videos from multiple cameras easily.
  /// </summary>
  public class VideoCaptureManager : MonoBehaviour
  {

    #region Properties

    [Header("Capture Controls")]
    // Start capture on awake if set to true.
    [SerializeField]
    public bool startOnAwake = false;
    // Quit process after capture finish.
    [SerializeField]
    public bool quitAfterCapture = false;
    // Get or set the current status.
    public CaptureStatus status { get; protected set; }
    // The capture duration if start capture on awake.
    [SerializeField]
    public float captureTime = 30f;

    [Header("Capture Options")]

    [Tooltip("Save folder for recorded video")]
    // Save path for recorded video including file name (c://xxx.mp4)
    [SerializeField]
    public string saveFolder = "";
    // The type of video capture mode, regular or 360.
    [SerializeField]
    public CaptureMode captureMode = CaptureMode.REGULAR;
    // The type of video projection, used for 360 video capture.
    [SerializeField]
    public ProjectionType projectionType = ProjectionType.NONE;
    // The type of video capture stereo mode, left right or top bottom.
    [SerializeField]
    public StereoMode stereoMode = StereoMode.NONE;
    // Stereo mode settings.
    // Average IPD of all subjects in US Army survey in meters
    [SerializeField]
    public float interpupillaryDistance = 0.0635f;
    // Audio capture settings, set false if you want to mute audio.
    [SerializeField]
    public bool captureAudio = true;
    // If audio captured from microphone
    [SerializeField]
    public bool captureMicrophone = false;
    // Microphone device index
    [SerializeField]
    public int deviceIndex = 0;
    // Setup Time.maximumDeltaTime to avoiding nasty stuttering.
    // https://docs.unity3d.com/ScriptReference/Time-maximumDeltaTime.html
    public bool offlineRender = false;

    [Tooltip("Encoding GPU acceleration will improve performance significantly, but only available for Windows with dedicated graphic card and H.264 codec.")]
    [SerializeField]
    public bool gpuEncoding = false;
    public EncoderPreset encoderPreset = EncoderPreset.H264_MP4;

    /// <summary>
    /// Encoding setting variables for video capture.
    /// </summary>
    [Header("Video Settings")]
    // Resolution preset settings, set custom for other resolutions
    [SerializeField]
    public ResolutionPreset resolutionPreset = ResolutionPreset.CUSTOM;
    [SerializeField]
    public CubemapFaceSize cubemapFaceSize = CubemapFaceSize._1024;
    [SerializeField]
    public Int32 frameWidth = 1280;
    [SerializeField]
    public Int32 frameHeight = 720;
    [Tooltip("Video bitrate in Kbps")]
    [SerializeField]
    public Int32 bitrate = 2000;
    [SerializeField]
    public Int16 frameRate = 30;
    [SerializeField]
    public AntiAliasingSetting antiAliasing = AntiAliasingSetting._1;

    [Header("Video Capture Components")]
    //[SerializeField]
    [SerializeField]
    public VideoCapture[] videoCaptures;

    public bool captureStarted { get; private set; }

    private string LOG_FORMAT = "[VideoCaptureManager] {0}";

    #endregion

    #region Video Capture

    public bool StartCapture()
    {
      if (captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Previous video capture manager session not finish yet!");
        return false;
      }

      // check all video capture is ready
      bool allReady = true;
      foreach (VideoCapture videoCapture in videoCaptures)
      {
        if (videoCapture.status != CaptureStatus.READY)
        {
          allReady = false;
          break;
        }
      }
      if (!allReady)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "There is one or more video capture session still in progress!");
        return false;
      }

      if (!FFmpegConfig.IsExist())
      {
        Debug.LogErrorFormat(LOG_FORMAT,
          "FFmpeg not found, please follow document and add ffmpeg executable before start capture!");
        return false;
      }

      saveFolder = Utils.CreateFolder(saveFolder);

      if (captureMode == CaptureMode._360)
      {
        if (projectionType == ProjectionType.NONE)
        {
          Debug.LogFormat(LOG_FORMAT,
            "Projection type should be set for 360 capture, set type to equirect for generating texture properly");
          projectionType = ProjectionType.EQUIRECT;
        }
        if (projectionType == ProjectionType.CUBEMAP)
        {
          if (stereoMode != StereoMode.NONE)
          {
            Debug.LogFormat(LOG_FORMAT,
              "Stereo settings not support for cubemap capture, reset to mono video capture.");
            stereoMode = StereoMode.NONE;
          }
        }
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      // start capture for all video capture
      foreach (VideoCapture videoCapture in videoCaptures)
      {
        // video capture settings
        videoCapture.startOnAwake = startOnAwake;
        videoCapture.captureTime = captureTime;
        videoCapture.quitAfterCapture = quitAfterCapture;
        videoCapture.captureMode = captureMode;
        videoCapture.projectionType = projectionType;
        // only VOD supported in multi capture
        videoCapture.captureType = CaptureType.VOD;
        videoCapture.saveFolder = saveFolder;
        videoCapture.resolutionPreset = resolutionPreset;
        videoCapture.frameWidth = frameWidth;
        videoCapture.frameHeight = frameHeight;
        videoCapture.frameRate = frameRate;
        videoCapture.bitrate = bitrate;
        videoCapture.stereoMode = stereoMode;
        videoCapture.interpupillaryDistance = interpupillaryDistance;
        videoCapture.cubemapFaceSize = cubemapFaceSize;
        videoCapture.offlineRender = offlineRender;
        videoCapture.captureAudio = captureAudio;
        videoCapture.captureMicrophone = captureMicrophone;
        videoCapture.deviceIndex = deviceIndex;
        videoCapture.antiAliasing = antiAliasing;
        videoCapture.gpuEncoding = gpuEncoding;
        videoCapture.encoderPreset = encoderPreset;

        videoCapture.StartCapture();
      }

      captureStarted = true;

      return true;
    }

    public bool StopCapture()
    {
      if (!captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Video capture manager session not start yet!");
        return false;
      }

      // stop all video capture started
      foreach (VideoCapture videoCapture in videoCaptures)
      {
        if (videoCapture.status == CaptureStatus.STARTED)
        {
          videoCapture.StopCapture();
        }
      }

      captureStarted = false;

      return true;
    }

    public bool CancelCapture()
    {
      if (!captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Video capture manager session not start yet!");
        return false;
      }

      // stop all video capture started
      foreach (VideoCapture videoCapture in videoCaptures)
      {
        if (videoCapture.status == CaptureStatus.STARTED)
        {
          videoCapture.CancelCapture();
        }
      }

      captureStarted = false;

      return true;
    }

    #endregion
  }
}