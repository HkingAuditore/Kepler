/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_2018_2_OR_NEWER
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>FFmpegEncoder</c> will capture the camera's render texture and encode it to video files by ffmpeg encoder.
  /// </summary>
  public class FFmpegEncoder : EncoderBase
  {
    #region Dll Import

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartVodCapture(
      EncoderPreset preset,
      int width,
      int height,
      int bitrate,
      int frameRate,
      bool verticalFlip,
      bool horizontalFlip,
      ProjectionType projectionType,
      StereoMode stereoMode,
      string videoPath,
      string ffmpegPath);

    [DllImport("FFmpegEncoder")]
    private static extern bool FFmpegEncoder_CaptureVodFrames(IntPtr api, byte[] data, int count);

    [DllImport("FFmpegEncoder")]
    unsafe private static extern bool FFmpegEncoder_CaptureVodFrames(IntPtr api, byte* data, int count);

    [DllImport("FFmpegEncoder")]
    private static extern bool FFmpegEncoder_StopVodCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanVodCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartLiveCapture(
      int width,
      int height,
      int bitrate,
      int frameRate,
      bool verticalFlip,
      bool horizontalFlip,
      ProjectionType projectionType,
      StereoMode stereoMode,
      string videoPath,
      string ffmpegPath);

    [DllImport("FFmpegEncoder")]
    private static extern bool FFmpegEncoder_CaptureLiveFrames(IntPtr api, byte[] data, int count);

    [DllImport("FFmpegEncoder")]
    unsafe private static extern bool FFmpegEncoder_CaptureLiveFrames(IntPtr api, byte* data, int count);

    [DllImport("FFmpegEncoder")]
    private static extern bool FFmpegEncoder_StopLiveCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    static extern void FFmpegEncoder_CleanLiveCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartScreenshot(
      int width,
      int height,
      bool verticalFlip,
      bool horizontalFlip,
      ProjectionType projectionType,
      StereoMode stereoMode,
      string imagePath,
      string ffmpegPath);

    [DllImport("FFmpegEncoder")]
    private static extern bool FFmpegEncoder_CaptureScreenshot(IntPtr api, byte[] data);

    [DllImport("FFmpegEncoder")]
    unsafe private static extern bool FFmpegEncoder_CaptureScreenshot(IntPtr api, byte* data);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_StopScreenshot(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanScreenshot(IntPtr api);

    #endregion

    #region Properties

    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // Vertical flip encode video
    private bool verticalFlip;
    // Horizontal flip encode video
    private bool horizontalFlip;

    // The delta time of each frame
    private float deltaFrameTime;
    // If the capture session is canceling.
    private bool captureCanceling;
    // The time spent during capturing.
    private float capturingTime;
    // Frame statistics info.
    private int capturedFrameCount;
    private int encodedFrameCount;
    // Reference to native encoder API
    private IntPtr nativeAPI;

    /// <summary>
    /// Frame data sent to frame encode queue.
    /// </summary>
    private unsafe struct FrameData
    {
      // The RGB pixels will be encoded.
      public byte[] pixels;
      public byte* unsafePixels;
      // How many this frame will be counted.
      public int count;
      // Constructor.
      public FrameData(byte[] pixels, int count)
      {
        this.pixels = pixels;
        this.count = count;
        this.unsafePixels = null;
      }
      // Unsafe constructor.
      public FrameData(byte* unsafePixels, int count)
      {
        this.pixels = null;
        this.count = count;
        this.unsafePixels = unsafePixels;
      }
    }
    // The frame encode queue.
    private Queue<FrameData> frameQueue;

    private bool supportsAsyncGPUReadback;
#if UNITY_2018_2_OR_NEWER
    /// <summary>
    /// GPU request data send to readback queue.
    /// </summary>
    private struct RequestData
    {
      // The GPU readback request.
      public AsyncGPUReadbackRequest request;
      // How many this frame will be counted.
      public int count;
      // Constructor.
      public RequestData(AsyncGPUReadbackRequest r, int c)
      {
        request = r;
        count = c;
      }
    }
    // The async frame request queue.
    private List<RequestData> requestQueue;
#endif

    // Video slice for live streaming.
    private string videoSlicePath;
    private int videoSliceCount;

    // The frame encode thread.
    private Thread encodeThread;
    // The async gpu readback thread.
    private Thread readbackThread;

    AutoResetEvent encodeReady = new AutoResetEvent(false);
    AutoResetEvent encodeComplete = new AutoResetEvent(false);

    public string ffmpegFullPath { get; set; }

    // Log message format template
    private string LOG_FORMAT = "[FFmpegEncoder] {0}";

    #endregion

    #region FFmpeg Encoder

    // Start capture video
    public bool StartCapture()
    {
      // Check camera setup
      bool cameraError = false;
      if (captureSource == CaptureSource.CAMERA)
      {
        if (!regularCamera)
        {
          cameraError = true;
        }
        if (stereoMode != StereoMode.NONE && !stereoCamera)
        {
          cameraError = true;
        }
      }

      if (cameraError)
      {
        OnError(EncoderErrorCode.CAMERA_SET_FAILED, null);
        return false;
      }

      // Check if we can start capture session
      if (captureStarted)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS, null);
        return false;
      }

      if (captureType == CaptureType.LIVE)
      {
        if (string.IsNullOrEmpty(liveStreamUrl))
        {
          OnError(EncoderErrorCode.INVALID_STREAM_URI, null);
          return false;
        }
      }

      if (captureMode != CaptureMode.REGULAR && captureSource == CaptureSource.RENDERTEXTURE)
      {
        Debug.LogFormat(LOG_FORMAT, "CaptureMode should be set regular for render texture capture");
        captureMode = CaptureMode.REGULAR;
      }

      if (captureMode == CaptureMode._360 && projectionType == ProjectionType.NONE)
      {
        Debug.LogFormat(LOG_FORMAT,
          "ProjectionType should be set for 360 capture, set type to equirect for generating texture properly");
        projectionType = ProjectionType.EQUIRECT;
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      // Calculate delta frame time based on frame rate
      deltaFrameTime = 1f / frameRate;

      // Check async GPU readback support
#if UNITY_2018_2_OR_NEWER
      supportsAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback;
#endif
      if (offlineRender || captureMode == CaptureMode._360)
      {
        supportsAsyncGPUReadback = false;
      }

      // Create texture for encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      if (captureType == CaptureType.VOD)
      {
        if (!captureAudio && customFileName != null)
        {
          videoSavePath = string.Format("{0}{1}.{2}",
            saveFolderFullPath,
            customFileName,
            Utils.GetEncoderPresetExt(encoderPreset));
        }
        else
        {
          videoSavePath = string.Format("{0}video_{1}x{2}_{3}_{4}.{5}",
            saveFolderFullPath,
            outputFrameWidth, outputFrameHeight,
            Utils.GetTimeString(),
            Utils.GetRandomString(5),
            Utils.GetEncoderPresetExt(encoderPreset));
        }
      }

      // Reset tempory variables.
      capturingTime = 0f;
      capturedFrameCount = 0;
      encodedFrameCount = 0;
      videoSliceCount = 0;
      captureCanceling = false;
      frameQueue = new Queue<FrameData>();
      if (supportsAsyncGPUReadback)
      {
#if UNITY_2018_2_OR_NEWER
        requestQueue = new List<RequestData>();
#endif
      }
      verticalFlip = true;
      if (captureSource == CaptureSource.SCREEN && supportsAsyncGPUReadback)
      {
        verticalFlip = false;
      }
      horizontalFlip = false;

      // Pass projection, stereo metadata into native plugin
      if (captureType == CaptureType.VOD)
      {
        nativeAPI = FFmpegEncoder_StartVodCapture(
          encoderPreset,
          outputFrameWidth,
          outputFrameHeight,
          bitrate,
          frameRate,
          verticalFlip,
          horizontalFlip,
          projectionType,
          stereoMode,
          videoSavePath,
          FFmpegConfig.path);

        if (nativeAPI == IntPtr.Zero)
        {
          OnError(EncoderErrorCode.VOD_FAILED_TO_START, null);
          return false;
        }
      }
      else if (captureType == CaptureType.LIVE)
      {
        videoSlicePath = string.Format("{0}{1}.h264",
          saveFolderFullPath,
          Utils.GetTimestampString());
        nativeAPI = FFmpegEncoder_StartLiveCapture(
          outputFrameWidth,
          outputFrameHeight,
          bitrate,
          frameRate,
          verticalFlip,
          horizontalFlip,
          projectionType,
          stereoMode,
          videoSlicePath,
          FFmpegConfig.path);

        if (nativeAPI == IntPtr.Zero)
        {
          OnError(EncoderErrorCode.LIVE_FAILED_TO_START, null);
          return false;
        }
      }

      // Update current status.
      captureStarted = true;

      // Sync with ffmpeg encode thread.
      StartCoroutine(SyncFrameQueue());

      if (encodeThread != null && encodeThread.IsAlive)
      {
        encodeThread.Abort();
        encodeThread = null;
      }

      // Start encoding thread.
      encodeThread = new Thread(FrameEncodeProcess);
      encodeThread.Start();

      //Debug.LogFormat(LOG_FORMAT, "FFmpegEncoder Started");

      return true;
    }

    /// <summary>
    /// Stop capture video.
    /// </summary>
    public bool StopCapture()
    {
      if (!captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Video capture session not start yet!");
        return false;
      }

      // Update current status.
      captureStarted = false;

      // Flush encode thread
      encodeReady.Set();

      //Debug.LogFormat(LOG_FORMAT, "FFmpegEncoder Stopped");
      //Debug.LogFormat(LOG_FORMAT, "CapturingTime: " + capturingTime);
      //Debug.LogFormat(LOG_FORMAT, "CapturedFrameCount: " + capturedFrameCount);

      return true;
    }

    /// <summary>
    /// Cancel capture video.
    /// </summary>
    public bool CancelCapture()
    {
      if (!captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Video capture session not start yet!");
        return false;
      }

      // Update current status.
      captureCanceling = true;
      captureStarted = false;

      StartCoroutine(CleanTempFiles());

      //Debug.LogFormat(LOG_FORMAT, "FFmpegEncoder Canceled");
      return true;
    }

    /// <summary>
    /// Configuration for Screenshot
    /// </summary>
    public bool StartScreenShot()
    {
      // Check if we can start capture session
      if (screenshotStarted)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS, null);
        return false;
      }

      if (captureMode != CaptureMode.REGULAR && captureSource == CaptureSource.RENDERTEXTURE)
      {
        Debug.LogFormat(LOG_FORMAT, "CaptureMode should be set regular for render texture capture");
        captureMode = CaptureMode.REGULAR;
      }

      if (captureMode == CaptureMode._360 && projectionType == ProjectionType.NONE)
      {
        Debug.LogFormat(LOG_FORMAT,
          "ProjectionType should be set for 360 capture, set type to equirect for generating texture properly");
        projectionType = ProjectionType.EQUIRECT;
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      // Calculate delta frame time based on frame rate
      deltaFrameTime = 1f / frameRate;

      // Create texture for encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      // If we haven't set the save path, we want to use project folder and timestamped file name by default
      screenshotSavePath = string.Format("{0}screenshot_{1}x{2}_{3}_{4}.jpg",
        saveFolderFullPath,
        outputFrameWidth, outputFrameHeight,
        Utils.GetTimeString(),
        Utils.GetRandomString(5));

      // Reset tempory variables.
      capturingTime = 0f;
      capturedFrameCount = 0;
      frameQueue = new Queue<FrameData>();
      if (supportsAsyncGPUReadback)
      {
#if UNITY_2018_2_OR_NEWER
        requestQueue = new List<RequestData>();
#endif
      }
      verticalFlip = true;
      if (captureSource == CaptureSource.SCREEN && supportsAsyncGPUReadback)
      {
        verticalFlip = false;
      }
      horizontalFlip = false;

      // Pass projection, stereo info into native plugin
      nativeAPI = FFmpegEncoder_StartScreenshot(
        outputFrameWidth,
        outputFrameHeight,
        verticalFlip,
        horizontalFlip,
        projectionType,
        stereoMode,
        screenshotSavePath,
        FFmpegConfig.path);
      if (nativeAPI == IntPtr.Zero)
      {
        OnError(EncoderErrorCode.SCREENSHOT_FAILED_TO_START, null);
        return false;
      }

      // Update current status.
      screenshotStarted = true;

      if (encodeThread != null && encodeThread.IsAlive)
      {
        encodeThread.Abort();
        encodeThread = null;
      }

      // Start encoding thread.
      encodeThread = new Thread(ScreenshotEncodeProcess);
      encodeThread.Start();

      //Debug.LogFormat(LOG_FORMAT, "FFmpegEncoder Started");

      return true;
    }

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
      OnError += EncoderErrorLog;
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled.
    /// </summary>
    private void OnDisable()
    {
      OnError -= EncoderErrorLog;
    }

    /// <summary>
    /// Called before any Start functions and also just after a prefab is instantiated.
    /// </summary>
    private void Awake()
    {
      captureStarted = false;
      screenshotStarted = false;

      supportsAsyncGPUReadback = false;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
      // Capture not started yet
      if (!captureStarted && !screenshotStarted)
        return;

      // Process async GPU readback request if supported
      if (supportsAsyncGPUReadback)
      {
        ProcessGPUReadbackRequestQueue();
      }

      capturingTime += Time.deltaTime;
      int totalRequiredFrameCount = (int)(capturingTime / deltaFrameTime);
      // Skip frames if we already got enough
      if (offlineRender || screenshotStarted || capturedFrameCount < totalRequiredFrameCount)
      {
        StartCoroutine(CaptureFrame());
      }
    }

    #endregion // Unity Lifecycle

    #region Encoder Internal

    // Capture video frame based on capture source and mode.
    private IEnumerator CaptureFrame()
    {
      yield return new WaitForEndOfFrame();

      if (captureSource == CaptureSource.CAMERA)
      {
        if (captureMode == CaptureMode.REGULAR)
        {
          regularCamera.Render();
          if (stereoMode != StereoMode.NONE)
          {
            stereoCamera.Render();
          }
          if (supportsAsyncGPUReadback)
          {
            SendGPUReadbackRequest();
          }
          else
          {
            CopyFrameTexture();
          }
        }
        else if (captureMode == CaptureMode._360)
        {
          if (supportsAsyncGPUReadback)
          {
            SendCubemapGPUReadbackRequest();
          }
          else
          {
            CopyCubemapFrameTexture();
          }
        }
      }
      else
      {
        if (supportsAsyncGPUReadback)
        {
          SendGPUReadbackRequest();
        }
        else
        {
          CopyFrameTexture();
        }
      }

      if (screenshotStarted)
      {
        if (!supportsAsyncGPUReadback)
        {
          screenshotStarted = false;
        }
      }
    }

    // Blit cubemap frame implementation.
    private void CopyCubemapFrameTexture()
    {
      if (projectionType == ProjectionType.CUBEMAP)
      {
        BlitCubemapTextures();
        CopyFrameTexture();
      }
      else if (projectionType == ProjectionType.EQUIRECT)
      {
        BlitEquirectTextures();
        CopyFrameTexture();
      }
    }

    // Copy the frame texture from GPU to CPU.
    private void CopyFrameTexture()
    {
      // Save original render texture
      RenderTexture prevTexture = RenderTexture.active;

      if (captureSource == CaptureSource.SCREEN)
      {
        RenderTexture.active = null;
      }
      else if (captureSource == CaptureSource.RENDERTEXTURE)
      {
        // Bind user input texture.
        RenderTexture.active = inputTexture;
      }
      else if (captureSource == CaptureSource.CAMERA)
      {
        if (stereoMode == StereoMode.NONE)
        {
          // Bind camera render texture.
          RenderTexture.active = outputTexture;
        }
        else
        {
          // Stereo cubemap capture not support.
          if (captureMode == CaptureMode._360 && projectionType == ProjectionType.CUBEMAP)
            return;

          BlitStereoTextures();

          RenderTexture.active = stereoOutputTexture;
        }
      }

      // Enqueue frame texture
      Texture2D texture2D = Utils.CreateTexture(outputFrameWidth, outputFrameHeight, null);
      // TODO, using native plugin to avoid expensive step of copying pixel data from GPU to CPU.
      texture2D.ReadPixels(new Rect(0, 0, outputFrameWidth, outputFrameHeight), 0, 0, false);
      texture2D.Apply();

      // Restore RenderTexture states.
      RenderTexture.active = prevTexture;

      StartCoroutine(EnqueueFrameTexture(texture2D));
    }

    // Enqueue the frame texture to queue for encoding.
    private IEnumerator EnqueueFrameTexture(Texture2D texture)
    {
      // Let ReadPixels and GetRawTextureData has one frame in between
      yield return null;

      EnqueueFrameBuffer(texture);
    }

    private void EnqueueFrameBuffer(Texture2D texture)
    {
      int totalRequiredFrameCount = (int)(capturingTime / deltaFrameTime);
      int requiredFrameCount = totalRequiredFrameCount - capturedFrameCount;
      if (requiredFrameCount > 5)
        Debug.LogWarningFormat(LOG_FORMAT, "Dropped " + (requiredFrameCount - 1) + " frames, please consider lower the frame rate.");
      byte[] buffer = texture.GetRawTextureData();
      lock (frameQueue)
      {
        frameQueue.Enqueue(new FrameData(buffer, requiredFrameCount));
      }
      //#if UNITY_2018_2_OR_NEWER && (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
      //      unsafe
      //      {
      //        byte* buffer = (byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(texture.GetRawTextureData<byte>());
      //        lock (frameQueue)
      //        {
      //          frameQueue.Enqueue(new FrameData(buffer, requiredFrameCount));
      //        }
      //      }
      //#endif

      encodeReady.Set();

      capturedFrameCount = totalRequiredFrameCount;

      // Clean texture resources
      Destroy(texture);
    }

    // Sync with FFmpeg encode thread at the end of every frame.
    private IEnumerator SyncFrameQueue()
    {
      while (captureStarted)
      {
        yield return new WaitForEndOfFrame();
        while (frameQueue != null && frameQueue.Count > 4)
        {
          encodeComplete.WaitOne();
        }
      }
    }

    // Send GPU readback frame request.
    private void SendGPUReadbackRequest()
    {
#if UNITY_2018_2_OR_NEWER
      if (requestQueue.Count > 6)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Too many GPU readback requests, skip!");
        return;
      }

      int totalRequiredFrameCount = (int)(capturingTime / deltaFrameTime);
      int requiredFrameCount = totalRequiredFrameCount - capturedFrameCount;
      if (requiredFrameCount > 5)
        Debug.LogWarningFormat(LOG_FORMAT, "Dropped " + (requiredFrameCount - 1) + " frames, please consider lower the frame rate.");

      // Blit to a temporary texture and request readback on it.
      if (captureSource == CaptureSource.SCREEN)
      {
        BlitScreenTextures();

        if (outputTexture != null)
        {
          requestQueue.Add(new RequestData(AsyncGPUReadback.Request(outputTexture), requiredFrameCount));
        }
      }
      else if (captureSource == CaptureSource.RENDERTEXTURE)
      {
        if (inputTexture != null)
        {
          requestQueue.Add(new RequestData(AsyncGPUReadback.Request(inputTexture), requiredFrameCount));
        }
      }
      else if (captureSource == CaptureSource.CAMERA)
      {
        if (stereoMode == StereoMode.NONE)
        {
          if (outputTexture != null)
          {
            requestQueue.Add(new RequestData(AsyncGPUReadback.Request(outputTexture), requiredFrameCount));
          }
        }
        else
        {
          // Stereo cubemap capture not support.
          if (captureMode == CaptureMode._360 && projectionType == ProjectionType.CUBEMAP)
            return;

          BlitStereoTextures();

          if (stereoOutputTexture != null)
          {
            requestQueue.Add(new RequestData(AsyncGPUReadback.Request(stereoOutputTexture), requiredFrameCount));
          }
        }
      }

      capturedFrameCount = totalRequiredFrameCount;
#endif
    }

    // Send GPU readback cubemap frame request.
    private void SendCubemapGPUReadbackRequest()
    {
      if (projectionType == ProjectionType.CUBEMAP)
      {
        BlitCubemapTextures();
        SendGPUReadbackRequest();
      }
      else if (projectionType == ProjectionType.EQUIRECT)
      {
        BlitEquirectTextures();
        SendGPUReadbackRequest();
      }
    }

    // Process GPU readback request queue.
    private void ProcessGPUReadbackRequestQueue()
    {
#if UNITY_2018_2_OR_NEWER
      while (requestQueue.Count > 0)
      {
        // Check if the first entry in the queue is completed.
        if (!requestQueue[0].request.done)
        {
          // Detect out-of-order case (the second entry in the queue
          // is completed before the first entry).
          if (requestQueue.Count > 1 && requestQueue[1].request.done)
          {
            // We can't allow the out-of-order case, so force it to
            // be completed now.
            requestQueue[0].request.WaitForCompletion();
          }
          else
          {
            // Nothing to do with the queue.
            break;
          }
        }

        // Retrieve the first entry in the queue.
        var requestData = requestQueue[0];
        requestQueue.RemoveAt(0);
        var request = requestData.request;

        // Error detection
        if (request.hasError)
        {
          Debug.LogWarningFormat(LOG_FORMAT, "GPU readback request has error.");
          capturedFrameCount -= requestData.count;
          break;
        }
        else
        {
          byte[] buffer = request.GetData<byte>().ToArray();
          lock (frameQueue)
          {
            // Enqueue frame buffer data
            frameQueue.Enqueue(new FrameData(buffer, requestData.count));
          }
          //unsafe
          //{
          //  byte* buffer = (byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(request.GetData<byte>());
          //  lock (frameQueue)
          //  {
          //    // Enqueue frame buffer data
          //    frameQueue.Enqueue(new FrameData(buffer, requestData.count));
          //  }
          //}

          encodeReady.Set();

          if (screenshotStarted)
            screenshotStarted = false;
        }
      }
#endif
    }

    // Frame encoding process in thread
    private void FrameEncodeProcess()
    {
      while (captureStarted || frameQueue.Count > 0)
      {
        // Submit pixel data for encode
        if (frameQueue.Count > 0)
        {
          FrameData frame;
          lock (frameQueue)
          {
            frame = frameQueue.Dequeue();
          }
          if (captureType == CaptureType.VOD)
          {
            if (frame.pixels == null)
            {
              unsafe
              {
                if (!FFmpegEncoder_CaptureVodFrames(nativeAPI, frame.unsafePixels, frame.count))
                {
                  capturedFrameCount -= frame.count;
                }
              }
            }
            else
            {
              if (!FFmpegEncoder_CaptureVodFrames(nativeAPI, frame.pixels, frame.count))
              {
                capturedFrameCount -= frame.count;
              }
            }
          }
          else if (captureType == CaptureType.LIVE)
          {
            if (frame.pixels == null)
            {
              unsafe
              {
                FFmpegEncoder_CaptureLiveFrames(nativeAPI, frame.unsafePixels, frame.count);
              }
            }
            else
            {
              FFmpegEncoder_CaptureLiveFrames(nativeAPI, frame.pixels, frame.count);
            }
          }
          encodedFrameCount += frame.count;
          // Slice video into different files for live stream
          if (captureStarted && captureType == CaptureType.LIVE)
          {
            int sliceCycleFrameCount = (videoSliceCount + 1) * frameRate * Constants.LIVE_VIDEO_SLICE_SECONDS;
            if (encodedFrameCount >= sliceCycleFrameCount)
            {
              FFmpegEncoder_StopLiveCapture(nativeAPI);
              FFmpegEncoder_CleanLiveCapture(nativeAPI);
              // Synce audio slice
              if (AudioRecorder.singleton != null)
              {
                AudioRecorder.singleton.SetLiveSyncCycle();
              }
              // Enqueue video slice
              FFmpegStreamer.singleton.EnqueueVideoSlice(videoSlicePath);
              videoSliceCount++;
              // Restart video slice encode
              videoSlicePath = string.Format("{0}{1}.h264",
                saveFolderFullPath,
                Utils.GetTimestampString());

              nativeAPI = FFmpegEncoder_StartLiveCapture(
                outputFrameWidth,
                outputFrameHeight,
                bitrate,
                frameRate,
                verticalFlip,
                horizontalFlip,
                projectionType,
                stereoMode,
                videoSlicePath,
                ffmpegFullPath);

              if (nativeAPI == IntPtr.Zero)
              {
                OnError(EncoderErrorCode.LIVE_FAILED_TO_START, null);
                break;
              }
            }
          }
          encodeComplete.Set();
        }
        else
        {
          // Wait new captured frames
          encodeReady.WaitOne();
        }
      }
      // Check if capture is canceled
      if (captureCanceling)
      {
        lock (frameQueue)
        {
          while (frameQueue.Count > 0)
            frameQueue.Dequeue();
        }
      }
      // Notify native encoding process finish
      if (captureType == CaptureType.VOD)
      {
        FFmpegEncoder_StopVodCapture(nativeAPI);
        FFmpegEncoder_CleanVodCapture(nativeAPI);
      }
      else if (captureType == CaptureType.LIVE)
      {
        FFmpegEncoder_StopLiveCapture(nativeAPI);
        FFmpegEncoder_CleanLiveCapture(nativeAPI);
        // Enqueue the last video slice
        if (File.Exists(videoSlicePath))
        {
          FFmpegStreamer.singleton.EnqueueVideoSlice(videoSlicePath);
        }
      }

      if (captureCanceling)
      {
        captureCanceling = false;
      }
      else
      {
        // Delay on complete event
        Thread.Sleep(1000);
        // Notify caller video capture complete
        OnComplete(videoSavePath);
        //Debug.LogFormat(LOG_FORMAT, "Video encode process finish!");
      }

      // Clear
      frameQueue.Clear();
      frameQueue = null;

      if (supportsAsyncGPUReadback)
      {
#if UNITY_2018_2_OR_NEWER
        requestQueue.Clear();
        requestQueue = null;
#endif
      }
    }

    // Screenshot encode process in thread
    private void ScreenshotEncodeProcess()
    {
      //Debug.LogFormat(LOG_FORMAT, "Encoding thread started!");
      while (screenshotStarted)
      {
        encodeReady.WaitOne();
        if (frameQueue.Count > 0)
        {
          FrameData frame;
          lock (frameQueue)
          {
            frame = frameQueue.Dequeue();
          }
          if (frame.pixels == null)
          {
            unsafe
            {
              FFmpegEncoder_CaptureScreenshot(nativeAPI, frame.unsafePixels);
            }
          }
          else
          {
            FFmpegEncoder_CaptureScreenshot(nativeAPI, frame.pixels);
          }
        }
        FFmpegEncoder_StopScreenshot(nativeAPI);
        FFmpegEncoder_CleanScreenshot(nativeAPI);

        OnComplete(screenshotSavePath);
      }
      //Debug.LogFormat(LOG_FORMAT, "Video encode process finish!");
    }

    private void EncoderErrorLog(EncoderErrorCode error, EncoderStatus? encoderStatus)
    {
      Debug.LogWarningFormat(LOG_FORMAT, "Error Occured of type: " + error + (encoderStatus != null ? " [Error code: " + encoderStatus + " ]" : ""));
    }

    private void BlitCubemapTextures()
    {
      regularCamera.RenderToCubemap(cubemapTexture);

      cubemapMaterial.SetTexture("_CubeTex", cubemapTexture);
      cubemapMaterial.SetVector("_SphereScale", sphereScale);
      cubemapMaterial.SetVector("_SphereOffset", sphereOffset);
      if (includeCameraRotation)
      {
        // cubemaps are always rendered along axes, so we do rotation by rotating the cubemap lookup
        cubemapMaterial.SetMatrix("_CubeTransform", Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one));
      }
      else
      {
        cubemapMaterial.SetMatrix("_CubeTransform", Matrix4x4.identity);
      }
      cubemapMaterial.SetPass(0);

      Graphics.SetRenderTarget(cubemapRenderTarget);

      float s = 1.0f / 3.0f;
      RenderCubeFace(CubemapFace.PositiveX, 0.0f, 0.5f, s, 0.5f);
      RenderCubeFace(CubemapFace.NegativeX, s, 0.5f, s, 0.5f);
      RenderCubeFace(CubemapFace.PositiveY, s * 2.0f, 0.5f, s, 0.5f);

      RenderCubeFace(CubemapFace.NegativeY, 0.0f, 0.0f, s, 0.5f);
      RenderCubeFace(CubemapFace.PositiveZ, s, 0.0f, s, 0.5f);
      RenderCubeFace(CubemapFace.NegativeZ, s * 2.0f, 0.0f, s, 0.5f);

      Graphics.SetRenderTarget(null);
      Graphics.Blit(cubemapRenderTarget, outputTexture);
    }

    private void BlitEquirectTextures()
    {
      regularCamera.RenderToCubemap(equirectTexture);
      regularCamera.Render();
      if (includeCameraRotation)
      {
        equirectMaterial.SetMatrix("_CubeTransform", Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one));
      }
      else
      {
        equirectMaterial.SetMatrix("_CubeTransform", Matrix4x4.identity);
      }
      // Convert to equirectangular projection.
      Graphics.Blit(equirectTexture, outputTexture, equirectMaterial);
      // From frameRenderTexture to frameTexture.
      if (stereoMode != StereoMode.NONE)
      {
        stereoCamera.RenderToCubemap(stereoEquirectTexture);
        stereoCamera.Render();
        // Convert to equirectangular projection.
        Graphics.Blit(stereoEquirectTexture, stereoTexture, equirectMaterial);
      }
    }

    protected new void CreateStereoTextures()
    {
      base.CreateStereoTextures();

      if (captureSource == CaptureSource.CAMERA && stereoMode != StereoMode.NONE)
      {
        stereoCamera.targetTexture = stereoTexture;
      }
    }

    private IEnumerator CleanTempFiles()
    {
      while (captureCanceling)
        yield return new WaitForSeconds(1);

      if (File.Exists(videoSavePath))
        File.Delete(videoSavePath);
      videoSavePath = "";

      if (File.Exists(videoSlicePath))
        File.Delete(videoSlicePath);
      videoSlicePath = "";
    }

    #endregion
  }
}