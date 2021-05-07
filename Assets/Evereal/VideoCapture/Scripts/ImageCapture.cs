/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.IO;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>ImageCapture</c> component, capture single image.
  /// </summary>
  [Serializable, ExecuteInEditMode]
  public class ImageCapture : CaptureBase
  {
    #region Properties

    // The output image format.
    [SerializeField]
    public ImageFormat imageFormat = ImageFormat.PNG;
    [SerializeField]
    public int jpgQuality = 75;

    public string imageSavePath { get; protected set; }

    // The image write thread.
    private Thread writeImageThread;
    // The image date.
    private byte[] imageData;

    #endregion

    #region Image Capture

    public override bool StartCapture()
    {
      // Sequence capture only support VOD now
      captureType = CaptureType.VOD;

      if (!PrepareCapture())
      {
        return false;
      }

      string ext = imageFormat == ImageFormat.PNG ? "png" : "jpg";
      imageSavePath = string.Format("{0}image_{1}.{2}",
        saveFolderFullPath,
        Utils.GetTimeString(),
        ext);

      imageData = null;

      // Create texture for encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      status = CaptureStatus.STARTED;

      StartCoroutine(CaptureImage());

      if (writeImageThread != null && writeImageThread.IsAlive)
      {
        writeImageThread.Abort();
        writeImageThread = null;
      }

      // Start encoding thread.
      writeImageThread = new Thread(WriteImageProcess);
      writeImageThread.Priority = System.Threading.ThreadPriority.Lowest;
      writeImageThread.IsBackground = true;
      writeImageThread.Start();

      Debug.LogFormat(LOG_FORMAT, "Image capture session started.");

      return true;
    }

    private IEnumerator CaptureImage()
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

    private void CaptureFrame()
    {
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
      if (imageFormat == ImageFormat.PNG)
      {
        // Encode texture into PNG
        imageData = texture2D.EncodeToPNG();
      }
      else
      {
        // Encode texture into JPG
        imageData = texture2D.EncodeToJPG(jpgQuality);
      }

      // Clean texture resources
      if (Application.isEditor)
      {
        DestroyImmediate(texture2D);
      }
      else
      {
        Destroy(texture2D);
      }

      // Reset camera settings.
      ResetCameraSettings();
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
    /// Write image data process in thread.
    /// </summary>
    private void WriteImageProcess()
    {
      while (imageData == null)
      {
        Thread.Sleep(100);
      }
      File.WriteAllBytes(imageSavePath, imageData);

      status = CaptureStatus.READY;

      Debug.LogFormat(LOG_FORMAT, "Image capture session success!");

      imageData = null;

      EnqueueCompleteEvent(new CaptureCompleteEventArgs(imageSavePath));
    }

    #endregion
  }
}