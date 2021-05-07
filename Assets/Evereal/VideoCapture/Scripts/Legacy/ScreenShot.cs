/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
//using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>ScreenShot</c> component, manage and record game screenshot from specific camera.
  /// Work with software encoder or GPU encoder component to generate screenshot.
  /// </summary>
  [Serializable]
  public class ScreenShot : MonoBehaviour
  {
    #region Properties

    private bool _captureStarted;

    public bool captureStarted
    {
      get
      {
        return _captureStarted;
      }
    }

    [Header("Capture Options")]

    //// You can choose capture from camera, screen or render texture.
    //[SerializeField]
    //public CaptureSource captureSource = CaptureSource.CAMERA;
    // Save path for recorded video including file name (c://xxx.jpg)
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
    public float interpupillaryDistance = 0.0635f;

    /// <summary>
    /// Encoding setting variables for image capture.
    /// </summary>
    [Header("Screenshot Settings")]
    // Resolution preset settings, set custom for other resolutions
    public ResolutionPreset resolutionPreset = ResolutionPreset.CUSTOM;
    [SerializeField]
    public CubemapFaceSize cubemapFaceSize = CubemapFaceSize._1024;
    private Int32 cubemapSize = 1024;
    [SerializeField]
    public Int32 frameWidth = 1280;
    [SerializeField]
    public Int32 frameHeight = 720;
    public AntiAliasingSetting antiAliasingSetting = AntiAliasingSetting._1;
    private Int16 antiAliasing = 1;

    [Header("Capture Cameras")]
    // The camera render content will be used for capturing video.
    [Tooltip("Reference to camera that renders regular video")]
    [SerializeField]
    public Camera regularCamera;
    [Tooltip("Reference to camera that renders other eye for stereo capture")]
    [SerializeField]
    public Camera stereoCamera;

    /// <summary>
    /// Encoder components for video encoding.
    /// </summary>
    [Header("Encoder Settings")]
    // Use hardware gpu encoding
    [SerializeField]
    public bool gpuEncoding = false;
    // FFmpeg Encoder
    public FFmpegEncoder ffmpegEncoder;
    // GPU Encoder
    public GPUEncoder gpuEncoder;

    // The garbage collection thread.
    //private Thread garbageCollectionThread;
    //private static bool garbageThreadRunning = false;

    // The save folder full path
    private string saveFolderFullPath = "";

    // Log message format template
    private string LOG_FORMAT = "[ScreenShot] {0}";

    #endregion

    #region Events

    protected Queue<CaptureCompleteEventArgs> completeEventQueue = new Queue<CaptureCompleteEventArgs>();
    protected Queue<CaptureErrorEventArgs> errorEventQueue = new Queue<CaptureErrorEventArgs>();

    public event EventHandler<CaptureCompleteEventArgs> OnComplete;

    protected void OnCaptureComplete(CaptureCompleteEventArgs args)
    {
      EventHandler<CaptureCompleteEventArgs> handler = OnComplete;
      if (handler != null)
      {
        handler(this, args);
      }
    }

    public event EventHandler<CaptureErrorEventArgs> OnError;

    protected void OnCaptureError(CaptureErrorEventArgs args)
    {
      EventHandler<CaptureErrorEventArgs> handler = OnError;
      if (handler != null)
      {
        handler(this, args);
      }
    }

    #endregion

    #region Screenshot

    /// <summary>
    /// Initialize the attributes of the capture session and start capture.
    /// </summary>
    public bool StartCapture()
    {
      if (_captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Previous screenshot session not finish yet!");
        OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.SCREENSHOT_ALREADY_IN_PROGRESS));
        return false;
      }

      saveFolderFullPath = Utils.CreateFolder(saveFolder);

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
        CubemapSizeSettings();
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      AntiAliasingSettings();

      // init ffmpeg encoding settings
      FFmpegEncoderSettings();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

      if (gpuEncoding)
      {
        if (FreeTrial.Check())
        {
          Debug.LogFormat(LOG_FORMAT, "GPU encoding is not supported in free trial version, fall back to software encoding.");
          gpuEncoding = false;
        }

        // init GPU encoding settings
        GPUEncoderSettings();

        if (!gpuEncoder.instantiated || !gpuEncoder.IsSupported())
        {
          Debug.LogFormat(LOG_FORMAT, "GPU encoding is not supported in current device or settings, fall back to software encoding.");
          gpuEncoding = false;
        }
      }

#else

      if (gpuEncoding) {
        Debug.LogFormat(LOG_FORMAT, "GPU encoding is only available on windows system, fall back to software encoding.");
        gpuEncoding = false;
      }

#endif

      if (gpuEncoding)
      {
        // init hardware encoding settings
        GPUEncoderSettings();

        if (!gpuEncoder.StartScreenShot())
        {
          OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.SCREENSHOT_START_FAILED));
          return false;
        }
      }
      else
      {
        if (!ffmpegEncoder.StartScreenShot())
        {
          OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.SCREENSHOT_START_FAILED));
          return false;
        }
      }

      _captureStarted = true;

      // Start garbage collect thread.
      //if (!garbageThreadRunning)
      //{
      //  garbageThreadRunning = true;

      //  if (garbageCollectionThread != null &&
      //    garbageCollectionThread.IsAlive)
      //  {
      //    garbageCollectionThread.Abort();
      //    garbageCollectionThread = null;
      //  }

      //  garbageCollectionThread = new Thread(GarbageCollectionProcess);
      //  garbageCollectionThread.Priority = System.Threading.ThreadPriority.Lowest;
      //  garbageCollectionThread.IsBackground = true;
      //  garbageCollectionThread.Start();
      //}

      Debug.LogFormat(LOG_FORMAT, "Screen shot session started.");
      return true;
    }

    private void GPUEncoderSettings()
    {
      gpuEncoder.regularCamera = regularCamera;
      gpuEncoder.stereoCamera = stereoCamera;
      gpuEncoder.captureMode = captureMode;
      gpuEncoder.resolutionPreset = resolutionPreset;
      gpuEncoder.frameWidth = frameWidth;
      gpuEncoder.frameHeight = frameHeight;
      gpuEncoder.cubemapSize = cubemapSize;
      gpuEncoder.projectionType = projectionType;
      gpuEncoder.stereoMode = stereoMode;
      gpuEncoder.interpupillaryDistance = interpupillaryDistance;
      gpuEncoder.antiAliasing = antiAliasing;
      gpuEncoder.saveFolderFullPath = saveFolderFullPath;
    }

    private void FFmpegEncoderSettings()
    {
      ffmpegEncoder.regularCamera = regularCamera;
      ffmpegEncoder.stereoCamera = stereoCamera;
      ffmpegEncoder.captureMode = captureMode;
      ffmpegEncoder.resolutionPreset = resolutionPreset;
      ffmpegEncoder.frameWidth = frameWidth;
      ffmpegEncoder.frameHeight = frameHeight;
      ffmpegEncoder.cubemapSize = cubemapSize;
      ffmpegEncoder.projectionType = projectionType;
      ffmpegEncoder.stereoMode = stereoMode;
      ffmpegEncoder.interpupillaryDistance = interpupillaryDistance;
      ffmpegEncoder.antiAliasing = antiAliasing;
      ffmpegEncoder.ffmpegFullPath = FFmpegConfig.path;
      ffmpegEncoder.saveFolderFullPath = saveFolderFullPath;
    }

    private void CubemapSizeSettings()
    {
      if (cubemapFaceSize == CubemapFaceSize._512)
      {
        cubemapSize = 512;
      }
      else if (cubemapFaceSize == CubemapFaceSize._1024)
      {
        cubemapSize = 1024;
      }
      else if (cubemapFaceSize == CubemapFaceSize._2048)
      {
        cubemapSize = 2048;
      }
    }

    private void AntiAliasingSettings()
    {
      if (antiAliasingSetting == AntiAliasingSetting._1)
      {
        antiAliasing = 1;
      }
      else if (antiAliasingSetting == AntiAliasingSetting._2)
      {
        antiAliasing = 2;
      }
      else if (antiAliasingSetting == AntiAliasingSetting._4)
      {
        antiAliasing = 4;
      }
      else if (antiAliasingSetting == AntiAliasingSetting._8)
      {
        antiAliasing = 8;
      }
    }

    public FFmpegEncoder GetFFmpegEncoder()
    {
      return ffmpegEncoder;
    }

    public GPUEncoder GetGPUEncoder()
    {
      return gpuEncoder;
    }

    /// <summary>
    /// Handle callbacks for the video encoder complete.
    /// </summary>
    /// <param name="savePath">Image save path.</param>
    private void OnEncoderComplete(string savePath)
    {
      OnCaptureComplete(new CaptureCompleteEventArgs(savePath));

      _captureStarted = false;

      Debug.LogFormat(LOG_FORMAT, "Screen shot session success!");
    }

    ///// <summary>
    ///// Garbage collection thread function.
    ///// </summary>
    //void GarbageCollectionProcess()
    //{
    //  Thread.Sleep(1000);
    //  System.GC.Collect();

    //  garbageThreadRunning = false;
    //}

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
      if (ffmpegEncoder == null)
      {
        ffmpegEncoder = GetComponentInChildren<FFmpegEncoder>(true);
        if (ffmpegEncoder == null)
        {
          Debug.LogErrorFormat(LOG_FORMAT,
           "Component software Encoder not found, please use prefab or follow the document to set up video capture.");
        }
      }

      if (gpuEncoder == null)
      {
        gpuEncoder = GetComponentInChildren<GPUEncoder>(true);
        if (gpuEncoder == null)
        {
          Debug.LogErrorFormat(LOG_FORMAT,
           "Component GPU encoder not found, please use prefab or follow the document to set up video capture.");
        }
      }

      if (ffmpegEncoder != null)
        ffmpegEncoder.OnComplete += OnEncoderComplete;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (gpuEncoder != null)
      {
        gpuEncoder.gameObject.SetActive(true);
        gpuEncoder.OnComplete += OnEncoderComplete;
      }
#endif
    }

    private void OnDestroy()
    {
      if (ffmpegEncoder != null)
        ffmpegEncoder.OnComplete -= OnEncoderComplete;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (gpuEncoder != null)
        gpuEncoder.OnComplete -= OnEncoderComplete;
#endif
    }

    #endregion
  }
}