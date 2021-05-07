/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>SequenceCapture</c> component, capture image sequence frame by frame.
  /// </summary>
  [Serializable]
  public class SequenceCapture : CaptureBase
  {
    #region Dll Import

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartSeqEncodeProcess(
      EncoderPreset ep,
      int rate,
      string path,
      string sequence,
      string ffpath);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanSeqEncodeProcess(IntPtr api);

    #endregion

    #region Properties

    // The output image format.
    [SerializeField]
    public ImageFormat imageFormat = ImageFormat.PNG;
    [SerializeField]
    public int jpgQuality = 75;
    // If encode to video after capture sequence.
    [SerializeField]
    public bool encodeFromImages;
    // If remove images after encoded to video.
    [SerializeField]
    public bool cleanImages;

    // Counter for image sequence
    private int imageIndex = 1;

    // Uniq timestring for sequence image file name
    private String startTimeString;

    // The frame encode thread.
    private Thread encodeThread;

    #endregion

    #region Sequence Capture

    public override bool StartCapture()
    {
      // Sequence capture only support VOD now
      captureType = CaptureType.VOD;

      if (!PrepareCapture())
      {
        return false;
      }

      imageIndex = 1;
      startTimeString = Utils.GetTimeString();

      Time.captureFramerate = frameRate;

      // Create texture for encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      // Create a blitter object to keep frames presented on the screen
      if (screenBlitter)
        CreateBlitterInstance();

      status = CaptureStatus.STARTED;

      StartCoroutine(CaptureFrames());

      Debug.LogFormat(LOG_FORMAT, "Sequence capture session started.");
      return true;
    }

    public override bool StopCapture()
    {
      if (status != CaptureStatus.STARTED)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Sequence capture session not start yet!");
        return false;
      }

      Time.captureFramerate = 0;

      if (encodeFromImages)
      {
        status = CaptureStatus.PENDING;

        if (encodeThread != null && encodeThread.IsAlive)
        {
          encodeThread.Abort();
          encodeThread = null;
        }

        // Start encoding thread.
        encodeThread = new Thread(SequenceEncodeProcess);
        encodeThread.Priority = System.Threading.ThreadPriority.Normal;
        //encodeThread.IsBackground = true;
        encodeThread.Start();
      }
      else
      {
        Debug.LogFormat(LOG_FORMAT, "Sequence capture session success!");

        status = CaptureStatus.READY;

        OnCaptureComplete(new CaptureCompleteEventArgs(GetImageSavePath(1)));
      }

      // Restore screen render.
      if (screenBlitter)
        ClearBlitterInstance();

      // Reset camera settings.
      ResetCameraSettings();

      return true;
    }

    public override bool CancelCapture()
    {
      if (status != CaptureStatus.STARTED)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Sequence capture session not start yet!");
        return false;
      }

      Time.captureFramerate = 0;

      // Restore screen render.
      ClearBlitterInstance();

      // Reset camera settings.
      ResetCameraSettings();

      status = CaptureStatus.READY;

      Debug.LogFormat(LOG_FORMAT, "Sequence capture session canceled.");

      return true;
    }

    private IEnumerator CaptureFrames()
    {
      while (status == CaptureStatus.STARTED)
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
            CaptureFrame();
          }
          else
          {
            CaptureCubemapFrame();
          }
        }
        else
        {
          CaptureFrame();
        }
      }
    }

    private void CaptureFrame()
    {
      string savePath = GetImageSavePath(imageIndex);
      imageIndex++;

      // Save original render texture
      RenderTexture prevTexture = RenderTexture.active;

      if (captureSource == CaptureSource.CAMERA)
      {
        // Mono capture
        if (stereoMode == StereoMode.NONE)
        {
          // Bind camera render texture.
          RenderTexture.active = outputTexture;
        }
        else // Stereo capture
        {
          // Stereo cubemap capture not support.
          if (captureMode == CaptureMode._360 && projectionType == ProjectionType.CUBEMAP)
            return;

          BlitStereoTextures();

          RenderTexture.active = stereoOutputTexture;
        }
      }
      else if (captureSource == CaptureSource.RENDERTEXTURE)
      {
        // Bind user input texture.
        RenderTexture.active = inputTexture;
      }
      else if (captureSource == CaptureSource.SCREEN)
      {
        RenderTexture.active = null;
      }

      Texture2D texture2D = Utils.CreateTexture(outputFrameWidth, outputFrameHeight, null);
      // Read screen contents into the texture
      texture2D.ReadPixels(new Rect(0, 0, outputFrameWidth, outputFrameHeight), 0, 0);
      texture2D.Apply();

      // Restore RenderTexture states.
      RenderTexture.active = prevTexture;

      // Encode image for write
      byte[] bytes;
      if (imageFormat == ImageFormat.PNG)
      {
        // Encode texture into PNG
        bytes = texture2D.EncodeToPNG();
      }
      else
      {
        // Encode texture into JPG
        bytes = texture2D.EncodeToJPG(jpgQuality);
      }

      // Clean texture resources
      Destroy(texture2D);

      File.WriteAllBytes(savePath, bytes);
    }

    /// <summary>
    /// Capture cubemap frame implementation
    /// </summary>
    private void CaptureCubemapFrame()
    {
      if (projectionType == ProjectionType.CUBEMAP)
      {
        regularCamera.RenderToCubemap(cubemapTexture);

        cubemapMaterial.SetTexture("_CubeTex", cubemapTexture);
        cubemapMaterial.SetVector("_SphereScale", sphereScale);
        cubemapMaterial.SetVector("_SphereOffset", sphereOffset);
        cubemapMaterial.SetMatrix("_CubeTransform", Matrix4x4.identity);
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

        CaptureFrame();
      }
      else if (projectionType == ProjectionType.EQUIRECT)
      {
        regularCamera.RenderToCubemap(equirectTexture);
        regularCamera.Render();
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
        CaptureFrame();
      }
    }

    /// <summary>
    /// Sequence image encoding process in thread.
    /// </summary>
    private void SequenceEncodeProcess()
    {
      Debug.LogFormat(LOG_FORMAT, "Sequence images encoding session started...");
      string videoSavePath = string.Format("{0}sequence_{1}x{2}_{3}.{4}",
        saveFolderFullPath,
        outputFrameWidth, outputFrameHeight,
        Utils.GetTimeString(),
        Utils.GetEncoderPresetExt(encoderPreset));

      IntPtr nativeAPI = FFmpegEncoder_StartSeqEncodeProcess(
        encoderPreset,
        frameRate,
        videoSavePath,
        string.Format("{0}sequence_{1}_%08d.{2}", saveFolderFullPath, startTimeString, imageFormat == ImageFormat.PNG ? "png" : "jpg"),
        ffmpegFullPath);
      if (nativeAPI == IntPtr.Zero)
      {
        EnqueueErrorEvent(new CaptureErrorEventArgs(CaptureErrorCode.SEQUENCE_ENCODE_START_FAILED));
        return;
      }
      // Clean resources
      FFmpegEncoder_CleanSeqEncodeProcess(nativeAPI);

      //string cmd = string.Format(
      //  " -r {0} -f image2 -i \"{1}\" -vcodec libvpx-vp9 \"{2}\"",
      //  frameRate,
      //  string.Format("{0}sequence_{1}_%08d.{2}", VideoSaveUtils.folder, startTimeString, imageFormat == ImageFormat.PNG ? "png" : "jpg"),
      //  videoSavePath
      //  );
      //Command.Run(ffmpegPath, cmd);

      if (cleanImages)
      {
        Debug.LogFormat(LOG_FORMAT, "Deleting sequence images...");
        for (int i = 1; i <= imageIndex; i++)
        {
          string imagePath = GetImageSavePath(i);
          if (File.Exists(imagePath))
          {
            File.Delete(imagePath);
          }
        }
      }

      status = CaptureStatus.READY;

      Debug.LogFormat(LOG_FORMAT, "Sequence capture session success!");

      EnqueueCompleteEvent(new CaptureCompleteEventArgs(videoSavePath));
    }

    /// <summary>
    /// Get the sequence image path for index.
    /// </summary>
    private string GetImageSavePath(int index)
    {
      string ext = imageFormat == ImageFormat.PNG ? "png" : "jpg";
      return string.Format("{0}sequence_{1}_{2}.{3}",
          saveFolderFullPath,
          startTimeString,
          index.ToString("00000000.##"),
          ext);
    }

    #endregion
  }
}