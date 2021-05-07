/* Copyright (c) 2020-present Evereal. All rights reserved. */

using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using uNvEncoder;

namespace Evereal.VideoCapture
{
  public class NvidiaEncoder : EncoderBase
  {
    #region Properties

    // Nvidia encoder instance
    public Encoder nvidiaEncoder = new Encoder();
    public bool forceIdrFrame = true;
    FileStream fileStream;
    BinaryWriter binaryWriter;

    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // The delta time of each frame
    private float deltaFrameTime;
    // The time spent during capturing.
    private float capturingTime;
    // Frame statistics info.
    private int capturedFrameCount;

    // Video slice for live streaming.
    private string videoSlicePath;
    //private int videoSliceCount;

    // Log message format template
    private string LOG_FORMAT = "[NvidiaEncoder] {0}";

    #endregion

    #region Nvidia Encoder

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

      // Create texture for encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      if (captureType == CaptureType.VOD)
      {
        videoSavePath = string.Format("{0}video_{1}x{2}_{3}_{4}.{5}",
            saveFolderFullPath,
            outputFrameWidth, outputFrameHeight,
            Utils.GetTimeString(),
            Utils.GetRandomString(5),
            "h264");
      }

      // Reset tempory variables.
      capturingTime = 0f;
      //videoSliceCount = 0;
      fileStream = new FileStream(videoSavePath, FileMode.Create, FileAccess.Write);
      binaryWriter = new BinaryWriter(fileStream);

      // Pass projection, stereo metadata into native plugin
      nvidiaEncoder.Create(outputFrameWidth, outputFrameHeight, frameRate);

      // Update current status.
      captureStarted = true;
      
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

      if (fileStream != null)
      {
        fileStream.Close();
      }

      if (binaryWriter != null)
      {
        binaryWriter.Close();
      }

      OnComplete(videoSavePath);

      return true;
    }

    /// <summary>
    /// Capture Cancel Routine with Unity resource release
    /// </summary>
    public void CancelCapture()
    {
      if (captureStarted)
      {
        //GPUEncoder_StopCapture();

        captureStarted = false;

        //StartCoroutine(CleanTempFiles());
      }
    }

    // Capture video frame based on capture source and mode.
    private IEnumerator CaptureFrame()
    {
      yield return new WaitForEndOfFrame();

      if (captureSource == CaptureSource.CAMERA)
      {
        if (captureMode == CaptureMode.REGULAR)
        {
          if (stereoMode != StereoMode.NONE)
          {
            stereoCamera.Render();
            BlitStereoTextures();
            Encode(stereoOutputTexture);
          }
          else
          {
            regularCamera.Render();
            Encode(outputTexture);
          }
        }
        else if (captureMode == CaptureMode._360)
        {
          if (projectionType == ProjectionType.CUBEMAP)
          {
            BlitCubemapTextures();
            Encode(outputTexture);
          }
          else if (projectionType == ProjectionType.EQUIRECT)
          {
            BlitEquirectTextures();
            if (stereoMode != StereoMode.NONE)
            {
              BlitStereoTextures();
              Encode(stereoOutputTexture);
            }
            else
            {
              Encode(outputTexture);
            }
          }
        }
      }
      else if (captureSource == CaptureSource.SCREEN)
      {
        BlitScreenTextures();
        Encode(outputTexture);
      }
      else if (captureSource == CaptureSource.RENDERTEXTURE)
      {
        Encode(inputTexture);
      }

      capturedFrameCount++;
    }

    void Encode(RenderTexture encodeTexture)
    {
      nvidiaEncoder.Update();
      nvidiaEncoder.Encode(encodeTexture, forceIdrFrame);
    }

    public void OnEncoded(System.IntPtr ptr, int size)
    {
      if (!captureStarted)
        return;

      var bytes = new byte[size];
      Marshal.Copy(ptr, bytes, 0, size);
      binaryWriter.Write(bytes);
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

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
      // Capture not started yet
      if (!captureStarted && !screenshotStarted)
        return;

      capturingTime += Time.deltaTime;
      int totalRequiredFrameCount = (int)(capturingTime / deltaFrameTime);
      // Skip frames if we already got enough
      if (offlineRender || screenshotStarted || capturedFrameCount < totalRequiredFrameCount)
      {
        StartCoroutine(CaptureFrame());
      }
    }

    void OnDisable()
    {
      captureStarted = false;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      nvidiaEncoder.Destroy();
#endif
    }

    void OnApplicationQuit()
    {
      if (fileStream != null)
      {
        fileStream.Close();
      }

      if (binaryWriter != null)
      {
        binaryWriter.Close();
      }
    }

    #endregion
  }
}