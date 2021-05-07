/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>GPUEncoder</c> will capture the camera's render texture and encode it to video files by GPU encoder.
  /// </summary>
  public class GPUEncoder : EncoderBase
  {

    #region Dll Import

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_Reset();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetLiveCaptureSettings(
      int width,
      int height,
      int frameRate,
      int bitRate,
      float flushCycleStart,
      float flushCycleAfter,
      string streamUrl,
      bool is360,
      bool verticalFlip,
      bool horizontalFlip,
      ProjectionType projectionType,
      StereoMode stereoMode);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetVodCaptureSettings(
      int width,
      int height,
      int frameRate,
      int bitRate,
      string fullSavePath,
      bool is360,
      bool verticalFlip,
      bool horizontalFlip,
      ProjectionType projectionType,
      StereoMode stereoMode);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetPreviewCaptureSettings(
      int width,
      int height,
      int frameRate,
      bool is360,
      bool verticalFlip,
      bool horizontalFlip);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetScreenshotSettings(
      int width,
      int height,
      string fullSavePath,
      bool is360,
      bool verticalFlip,
      bool horizontalFlip);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetCameraOverlaySettings(
      float widthPercentage,
      UInt32 viewPortTopLeftX,
      UInt32 viewPortTopLeftY);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_EnumerateMicDevices();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern UInt32 GPUEncoder_GetMicDevicesCount();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern string GPUEncoder_GetMicDeviceName(UInt32 index);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetMicDevice(UInt32 index);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_UnsetMicDevice();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetMicEnabledDuringCapture(bool enabled);
    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetAudioEnabledDuringCapture(bool enabled);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_EnumerateCameraDevices();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern UInt32 GPUEncoder_GetCameraDevicesCount();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern string GPUEncoder_GetCameraDeviceName(UInt32 index);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetCameraDevice(UInt32 index);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_UnsetCameraDevice();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetCameraEnabledDuringCapture(bool enabled);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SetMicAndAudioRenderDeviceByVRDeviceType(VRDeviceType vrDevice);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_StartLiveCapture();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_StartVodCapture();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_StartPreviewCapture();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_StartScreenshot();
    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_CaptureTexture(IntPtr texturePtr);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_PreviewCapture(IntPtr texturePtr);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_PreviewCamera(IntPtr texturePtr);

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_GetEncoderStatus();
    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_GetScreenshotStatus();

    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern void GPUEncoder_StopCapture();
    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_GetCaptureCapability();
    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern EncoderStatus GPUEncoder_SaveScreenShot(IntPtr texturePtr);
    [DllImport("GPUEncoder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern GraphicsCard GPUEncoder_CheckGPUManufacturer();

    #endregion

    #region Properties

    public static GPUEncoder singleton;
    public bool instantiated { get; private set; }

    /// <summary>
    /// Encoding setting variables.
    /// </summary>
    // Video initial flush cycle
    private const float encodingInitialFlushCycle = 5f;
    // Video flush cycle
    private const float encodingSecondaryFlushCycle = 5f;

    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    private VRDeviceType attachedHMD;

    // Log message format template
    private string LOG_FORMAT = "[GPUEncoder] {0}";

    #endregion

    #region GPU Encoder

    void Update()
    {
      if (!captureStarted && !screenshotStarted) return;

      if (captureMode == CaptureMode._360)
      {
        if (projectionType == ProjectionType.EQUIRECT)
        {
          // Render rgb cubemap
          if (regularCamera && equirectTexture)
          {
            regularCamera.RenderToCubemap(equirectTexture);
          }

          if (stereoMode != StereoMode.NONE && stereoCamera && stereoEquirectTexture)
          {
            stereoCamera.RenderToCubemap(stereoEquirectTexture);
          }
        }
        else if (projectionType == ProjectionType.CUBEMAP)
        {
          if (regularCamera && cubemapTexture)
          {
            //regularCamera.transform.position = transform.position;
            regularCamera.RenderToCubemap(cubemapTexture);
          }
        }
        StartCoroutine(BlitCubemapTextures());
      }
      else if (captureSource == CaptureSource.RENDERTEXTURE || captureSource == CaptureSource.SCREEN)
      {
        StartCoroutine(BlitRegularTextures());
      }
      else
      {
        regularCamera.Render();
        if (stereoMode != StereoMode.NONE)
        {
          stereoCamera.Render();
        }
        StartCoroutine(BlitRegularTextures());
      }
    }

    /// <summary>
    /// Start video capture
    /// </summary>
    public bool StartCapture()
    {
      EncoderStatus status = EncoderStatus.OK;

      // Check camera setup
      if (captureSource == CaptureSource.CAMERA)
      {
        if (captureMode == CaptureMode.REGULAR && !regularCamera)
        {
          status = EncoderStatus.CAMERA_SET_FAILED;
        }
        else if (captureMode == CaptureMode._360 && !regularCamera)
        {
          status = EncoderStatus.CAMERA_SET_FAILED;
        }
        if (stereoMode != StereoMode.NONE && !stereoCamera)
        {
          status = EncoderStatus.CAMERA_SET_FAILED;
        }
      }

      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.CAMERA_SET_FAILED, status);
        return false;
      }

      if (captureStarted || GPUEncoder_GetEncoderStatus() != EncoderStatus.OK)
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
          "ProjectionType should be set for 360 capture, et type to equirect for generating texture properly!");
        projectionType = ProjectionType.EQUIRECT;
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      // Check GPU capability for video encoding
      status = GPUEncoder_GetCaptureCapability();
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.VOD_FAILED_TO_START, status);
        return false;
      }

      // MAX video encoding resolution
      // AMD:     4096 x 2048
      // NVIDIA:  4096 x 4096
      if (GraphicsCard.AMD == GPUEncoder_CheckGPUManufacturer() &&
          (frameWidth > 4096 || frameHeight > 2048))
      {
        Debug.LogFormat(LOG_FORMAT, "Max video encoding resolution on AMD is 4096 x 2048");
        OnError(EncoderErrorCode.UNSUPPORTED_SPEC, null);
        return false;
      }
      else if (GraphicsCard.NVIDIA == GPUEncoder_CheckGPUManufacturer() &&
               (frameWidth > 4096 || frameHeight > 4096))
      {
        Debug.LogFormat(LOG_FORMAT, "Max video encoding resolution on NVIDIA is 4096 x 4096");
        OnError(EncoderErrorCode.UNSUPPORTED_SPEC, null);
        return false;
      }
      //else if (GraphicsCard.UNSUPPORTED_DEVICE == GPUEncoder_CheckGPUManufacturer())
      //{
      //  Debug.LogFormat(LOG_FORMAT,
      //    "Unsupported gpu device or you missed to call GPUEncoder_GetCaptureCapability supporting gpu device check");
      //  OnError(EncoderErrorCode.UNSUPPORTED_SPEC, null);
      //  return false;
      //}

      // Create RenderTextures which will be used for video encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      if (captureType == CaptureType.LIVE)
      {
        // Start live streaming video capture
        return StartLiveStreaming();
      }
      else
      {
        // Start VOD video capture
        return StartVodCapture();
      }
    }

    /// <summary>
    /// Configuration for Live Streaming
    /// </summary>
    private bool StartLiveStreaming()
    {
      EncoderStatus status = EncoderStatus.OK;

      if (string.IsNullOrEmpty(liveStreamUrl))
      {
        OnError(EncoderErrorCode.INVALID_STREAM_URI, null);
        return false;
      }

      // Video Encoding and Live Configuration Settings
      status = GPUEncoder_SetLiveCaptureSettings(
        width: outputFrameWidth,
        height: outputFrameHeight,
        frameRate: frameRate,
        bitRate: bitrate * 1000, // in bps
        flushCycleStart: encodingInitialFlushCycle,
        flushCycleAfter: encodingSecondaryFlushCycle,
        streamUrl: liveStreamUrl,
        is360: captureMode == CaptureMode._360 ? true : false,
        verticalFlip: captureSource == CaptureSource.SCREEN ? true : false,
        horizontalFlip: false,
        projectionType: projectionType,
        stereoMode: StereoMode.NONE);

      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.LIVE_FAILED_TO_START, status);
        return false;
      }

      // Pick attached audio device resources for audio capture
      status = GPUEncoder_SetMicAndAudioRenderDeviceByVRDeviceType(attachedHMD);
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.LIVE_FAILED_TO_START, status);
        return false;
      }

      // Make enable audio output capture(ex. speaker)
      status = GPUEncoder_SetAudioEnabledDuringCapture(captureAudio);
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.LIVE_FAILED_TO_START, status);
        return false;
      }

      // Make enable audio input capture(ex. microphone)
      status = GPUEncoder_SetMicEnabledDuringCapture(captureMicrophone);
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.LIVE_FAILED_TO_START, status);
        return false;
      }

      status = GPUEncoder_StartLiveCapture();
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.LIVE_FAILED_TO_START, status);
        return false;
      }

      OnEncoderStarted(EncoderCaptureType.LIVE);
      return true;
    }

    /// <summary>
    /// Configuration for Video Recording
    /// </summary>
    private bool StartVodCapture()
    {
      EncoderStatus status = EncoderStatus.OK;

      // If we haven't set the save path, we want to use project folder and timestamped file name by default
      if (customFileName != null)
      {
        videoSavePath = string.Format("{0}{1}.mp4",
          saveFolderFullPath,
          customFileName);
      }
      else
      {
        videoSavePath = string.Format("{0}capture_{1}x{2}_{3}_{4}.mp4",
          saveFolderFullPath,
          outputFrameWidth, outputFrameHeight,
          Utils.GetTimeString(),
          Utils.GetRandomString(5));
      }

      // Video Encoding Configuration Settings
      status = GPUEncoder_SetVodCaptureSettings(
        width: outputFrameWidth,
        height: outputFrameHeight,
        frameRate: frameRate,
        bitRate: bitrate * 1000, // in bps
        fullSavePath: videoSavePath,
        is360: captureMode == CaptureMode._360 ? true : false,
        verticalFlip: captureSource == CaptureSource.SCREEN ? true : false,
        horizontalFlip: false,
        projectionType: projectionType,
        stereoMode: StereoMode.NONE);

      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.VOD_FAILED_TO_START, status);
        return false;
      }

      // Pick attached audio device resources for audio capture
      status = GPUEncoder_SetMicAndAudioRenderDeviceByVRDeviceType(attachedHMD);
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.VOD_FAILED_TO_START, status);
        return false;
      }

      // Make enable audio output capture(ex. speaker)
      status = GPUEncoder_SetAudioEnabledDuringCapture(captureAudio);
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.VOD_FAILED_TO_START, status);
        return false;
      }

      // Make enable audio input capture(ex. microphone)
      status = GPUEncoder_SetMicEnabledDuringCapture(captureMicrophone);
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.VOD_FAILED_TO_START, status);
        return false;
      }

      // Start VOD capture
      status = GPUEncoder_StartVodCapture();
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.VOD_FAILED_TO_START, status);
        return false;
      }

      OnEncoderStarted(EncoderCaptureType.VOD);
      return true;
    }

    /// <summary>
    /// Configuration for Screenshot
    /// </summary>
    public bool StartScreenShot()
    {
      EncoderStatus status;

      // Check current screenshot status.
      // It should return EncoderStatus.OK when it's not in progress
      status = GPUEncoder_GetScreenshotStatus();
      if (screenshotStarted || status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS, null);
        return false;
      }

      // Check GPU capability for screenshot
      status = GPUEncoder_GetCaptureCapability();
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.SCREENSHOT_FAILED_TO_START, status);
      }

      if (captureMode != CaptureMode.REGULAR && captureSource == CaptureSource.RENDERTEXTURE)
      {
        Debug.LogFormat(LOG_FORMAT, "CaptureMode should be set regular for render texture capture");
        captureMode = CaptureMode.REGULAR;
      }

      if (captureMode == CaptureMode._360 && projectionType == ProjectionType.NONE)
      {
        Debug.LogFormat(LOG_FORMAT,
          "ProjectionType should be set for 360 capture, et type to equirect for generating texture properly!");
        projectionType = ProjectionType.EQUIRECT;
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      // Create RenderTextures which will be used for screenshot
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      // If we haven't set the save path, we want to use project folder and timestamped file name by default
      screenshotSavePath = string.Format("{0}/screenshot_{1}x{2}_{3}_{4}.jpg",
        saveFolderFullPath,
        outputFrameWidth, outputFrameHeight,
        Utils.GetTimeString(),
        Utils.GetRandomString(5));

      // Screenshot Configuration Settings in GPUEncoder
      status = GPUEncoder_SetScreenshotSettings(
        width: outputFrameWidth,
        height: outputFrameHeight,
        fullSavePath: screenshotSavePath,
        is360: captureMode == CaptureMode._360 ? true : false,
        verticalFlip: captureSource == CaptureSource.SCREEN ? true : false,
        horizontalFlip: false);

      // Start ScreenShot
      status = GPUEncoder_StartScreenshot();
      if (status != EncoderStatus.OK)
      {
        OnError(EncoderErrorCode.SCREENSHOT_FAILED_TO_START, status);
        return false;
      }

      if (captureMode == CaptureMode.REGULAR)
      {
        regularCamera.Render();
        if (stereoMode != StereoMode.NONE)
        {
          stereoCamera.Render();
        }
      }

      OnEncoderStarted(EncoderCaptureType.SCREENSHOT);
      return true;
    }

    /// <summary>
    /// Capture Stop Routine with Unity resource release
    /// </summary>
    public void StopCapture()
    {
      if (captureStarted)
      {

        GPUEncoder_StopCapture();

        captureStarted = false;
      }

      // Notify caller video capture complete
      if (captureType == CaptureType.VOD)
      {
        OnComplete(videoSavePath);
      }
      else if (captureType == CaptureType.LIVE)
      {
        OnComplete(liveStreamUrl);
      }
    }

    /// <summary>
    /// Capture Cancel Routine with Unity resource release
    /// </summary>
    public void CancelCapture()
    {
      if (captureStarted)
      {
        // TODO, add function GPUEncoder_CancelCapture()
        GPUEncoder_StopCapture();

        captureStarted = false;

        StartCoroutine(CleanTempFiles());
      }
    }

    /// <summary>
    /// Check if device support GPU encoding.
    /// </summary>
    /// <returns></returns>
    public bool IsSupported()
    {
      if (SystemInfo.graphicsDeviceVendor == "NVIDIA")
      {
        if (SystemInfo.graphicsDeviceName.Contains("RTX") ||
          SystemInfo.graphicsDeviceName.Contains("1080"))
        {
          return false;
        }
      }

      EncoderStatus status = GPUEncoder_GetCaptureCapability();
      // Check GPU capability for video encoding
      if (status != EncoderStatus.OK)
        return false;

      // MAX video encoding resolution
      // AMD:     4096 x 2048
      // NVIDIA:  4096 x 4096
      if (GraphicsCard.AMD == GPUEncoder_CheckGPUManufacturer() &&
          (frameWidth > 4096 || frameHeight > 2048))
        return false;
      else if (GraphicsCard.NVIDIA == GPUEncoder_CheckGPUManufacturer() &&
               (frameWidth > 4096 || frameHeight > 4096))
        return false;
      //else if (GraphicsCard.UNSUPPORTED_DEVICE == GPUEncoder_CheckGPUManufacturer())
      //  return false;

      return true;
    }

    IEnumerator BlitCubemapTextures()
    {

      yield return null;

      if (captureStarted || screenshotStarted)
      {

        yield return new WaitForEndOfFrame();

        regularCamera.targetTexture = outputTexture;

        if (projectionType == ProjectionType.EQUIRECT)
        {
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

          if (stereoMode != StereoMode.NONE)
          {
            // Convert to equirectangular projection.
            Graphics.Blit(stereoEquirectTexture, stereoTexture, equirectMaterial);
          }
        }
        else if (projectionType == ProjectionType.CUBEMAP)
        {
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

        yield return new WaitForEndOfFrame();

        // Pass captured texture for video or screenshot
        updateTextureToNative();
      }
    }

    IEnumerator BlitRegularTextures()
    {
      yield return new WaitForEndOfFrame();

      // Hack to fix screen shot first image black
      if (screenshotStarted)
        yield return new WaitForEndOfFrame();

      if (captureSource == CaptureSource.SCREEN)
      {
        BlitScreenTextures();
      }

      // Pass captured texture for video or screenshot
      updateTextureToNative();
    }

    private void updateTextureToNative()
    {
      EncoderStatus status;
      if ((inputTexture || outputTexture) && captureStarted)
      {
        if (captureSource == CaptureSource.RENDERTEXTURE)
        {
          // Passing input texture to GPUEncoder
          status = GPUEncoder_CaptureTexture(inputTexture.GetNativeTexturePtr());
        }
        else if (stereoMode != StereoMode.NONE)
        {
          BlitStereoTextures();

          // Passing render texture to GPUEncoder
          status = GPUEncoder_CaptureTexture(stereoOutputTexture.GetNativeTexturePtr());
        }
        else
        {
          // Passing render texture to GPUEncoder
          status = GPUEncoder_CaptureTexture(outputTexture.GetNativeTexturePtr());
        }

        if (status != EncoderStatus.OK)
        {
          OnError(EncoderErrorCode.TEXTURE_ENCODE_FAILED, status);
          StopCapture();
          return;
        }
      }

      if ((inputTexture || outputTexture) && screenshotStarted)
      {
        screenshotStarted = false;
        if (captureSource == CaptureSource.RENDERTEXTURE)
        {
          // Passing input texture to GPUEncoder
          status = GPUEncoder_SaveScreenShot(inputTexture.GetNativeTexturePtr());
        }
        else if (stereoMode != StereoMode.NONE)
        {
          BlitStereoTextures();

          // Hack to fix first screensot image black
          Texture2D tempTexture = Utils.CreateTexture(1, 1, null);
          RenderTexture.active = stereoOutputTexture;
          tempTexture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
          RenderTexture.active = null;
          Destroy(tempTexture);

          // Passing render texture to GPUEncoder
          status = GPUEncoder_SaveScreenShot(stereoOutputTexture.GetNativeTexturePtr());
        }
        else
        {
          // Passing render texture to GPUEncoder
          status = GPUEncoder_SaveScreenShot(outputTexture.GetNativeTexturePtr());
        }

        if (status != EncoderStatus.OK)
        {
          OnError(EncoderErrorCode.SCREENSHOT_FAILED, status);
          return;
        }
        else
        {
          OnComplete(screenshotSavePath);
          return;
        }
      }
    }

    private IEnumerator CleanTempFiles()
    {
      int waitCount = 0;
      while (!File.Exists(videoSavePath))
      {
        yield return new WaitForSeconds(1);
        if (++waitCount >= 5)
          break;
      }

      if (File.Exists(videoSavePath))
        File.Delete(videoSavePath);

      videoSavePath = "";
    }

    private void OnEncoderStarted(EncoderCaptureType captureType)
    {
      if (captureType == EncoderCaptureType.VOD || captureType == EncoderCaptureType.LIVE)
        captureStarted = true;
      else if (captureType == EncoderCaptureType.SCREENSHOT)
        screenshotStarted = true;
      //Debug.Log(string.Format("{0} capture started", captureType));
    }

    private void EncoderErrorLog(EncoderErrorCode error, EncoderStatus? encoderStatus)
    {
      Debug.LogWarningFormat(LOG_FORMAT, "Error Occured of type: " + error + (encoderStatus != null ? " [Error code: " + encoderStatus + " ]" : ""));
    }

    //void OnRenderImage(RenderTexture src, RenderTexture dst)
    //{
    //  if (captureMode == CaptureMode.REGULAR &&
    //    outputTexture && captureSource == CaptureSource.CAMERA)
    //  {
    //    Graphics.Blit(src, dst);
    //    Graphics.Blit(null, outputTexture);
    //    StartCoroutine(BlitRegularTextures());
    //  }
    //}

    void Awake()
    {
      if (singleton != null)
        return;
      singleton = this;
      instantiated = true;

      captureStarted = false;
      screenshotStarted = false;

      // Retrieve attached VR devie for sound and microphone capture in VR
      // If expected VR device is not attached, it will capture default audio device
#if UNITY_2019_1_OR_NEWER
      UnityEngine.XR.InputDevice inputDevice = new UnityEngine.XR.InputDevice();
      string vrDeviceName = string.IsNullOrEmpty(inputDevice.name) ? "" : inputDevice.name.ToLower();
#elif UNITY_2017_2_OR_NEWER
      string vrDeviceName = UnityEngine.XR.XRDevice.model.ToLower();
#else
      string vrDeviceName = "";
#endif

      if (vrDeviceName.Contains("rift"))
      {
        attachedHMD = VRDeviceType.OCULUS_RIFT;
      }
      else if (vrDeviceName.Contains("vive"))
      {
        attachedHMD = VRDeviceType.HTC_VIVE;
      }
      else
      {
        attachedHMD = VRDeviceType.UNKNOWN;
      }
    }

    private void OnEnable()
    {
      OnError += EncoderErrorLog;
    }

    private void OnDisable()
    {
      OnError -= EncoderErrorLog;
    }

    public UInt32 GetMicDevicesCount()
    {
      GPUEncoder_EnumerateMicDevices();
      return GPUEncoder_GetMicDevicesCount();
    }

    public string GetMicDeviceName(UInt32 index)
    {
      return GPUEncoder_GetMicDeviceName(index);
    }

    public void SetMicDevice(UInt32 index)
    {
      GPUEncoder_SetMicDevice(index);
    }

    public void UnsetMicDevice()
    {
      GPUEncoder_UnsetMicDevice();
    }

    public UInt32 GetCameraDevicesCount()
    {
      GPUEncoder_EnumerateCameraDevices();
      return GPUEncoder_GetCameraDevicesCount();
    }

    public string GetCameraDeviceName(UInt32 index)
    {
      return GPUEncoder_GetCameraDeviceName(index);
    }

    public void SetCameraDevice(UInt32 index)
    {
      GPUEncoder_SetCameraDevice(index);
    }

    public void UnsetCameraDevice()
    {
      GPUEncoder_UnsetCameraDevice();
    }

    public void SetCameraEnabledDuringCapture(bool enabled)
    {
      GPUEncoder_SetCameraEnabledDuringCapture(enabled);
    }

    public void SetCameraOverlaySettings(float widthPercentage, UInt32 viewPortTopLeftX, UInt32 viewPortTopLeftY)
    {
      GPUEncoder_SetCameraOverlaySettings(widthPercentage, viewPortTopLeftX, viewPortTopLeftY);
    }

    #endregion

  }
}
