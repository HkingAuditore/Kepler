/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Base capture class for Video Capture.
  /// </summary>
  public class CaptureBase : MonoBehaviour, ICapture
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

    // You can choose capture from camera, screen or render texture.
    [SerializeField]
    public CaptureSource captureSource = CaptureSource.CAMERA;
    // User custom input render texture
    [SerializeField]
    public RenderTexture inputTexture;
    // If set live streaming mode, encoded video will be push to remote streaming url instead of save to local file.
    [SerializeField]
    public CaptureType captureType = CaptureType.VOD;
    [Tooltip("Save folder for recorded video")]
    // Save folder path for recorded video.
    [SerializeField]
    public string saveFolder = "";
    // ex. rtmp://x.rtmp.youtube.com/live2/yourStreamKey
    [SerializeField]
    public string liveStreamUrl = "";
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
    // Cursor capture settings, set true if you want to capture cursor.
    [SerializeField]
    public bool captureCursor;
    // The capture cursor image.
    [SerializeField]
    public Texture2D cursorImage;
    // Audio capture settings, set false if you want to mute audio.
    [SerializeField]
    public bool captureAudio = true;
    // Capture microphone settings
    [SerializeField]
    public bool captureMicrophone = false;
    // Microphone device index
    [SerializeField]
    public int deviceIndex = 0;
    // Support transparent capture
    [SerializeField]
    public bool transparent = false;
    // Setup Time.captureFramerate to avoiding nasty stuttering.
    // https://docs.unity3d.com/ScriptReference/Time-maximumDeltaTime.html
    [SerializeField]
    public bool offlineRender = false;
    // Enable screen blitter
    [SerializeField]
    public bool screenBlitter = true;

    /// <summary>
    /// Capture setting variables for video capture.
    /// </summary>
    [Header("Capture Settings")]

    // Resolution preset settings, set custom for other resolutions
    [SerializeField]
    public ResolutionPreset resolutionPreset = ResolutionPreset.CUSTOM;
    [SerializeField]
    public CubemapFaceSize cubemapFaceSize = CubemapFaceSize._1024;
    protected Int32 cubemapSize = 1024;
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
    protected Int16 antiAliasingSetting = 1;

    //[Header("Capture Cameras")]

    // The camera render content will be used for capturing video.
    [Tooltip("Reference to camera that renders regular video")]
    [SerializeField]
    public Camera regularCamera;
    [Tooltip("Reference to camera that renders other eye for stereo capture")]
    [SerializeField]
    public Camera stereoCamera;
    // Camera settings.
    private Vector3 regularCameraPos = Vector3.zero;
    private CameraClearFlags regularCameraClearFlags = CameraClearFlags.Skybox;
    private Color regularCameraBackgroundColor = Color.blue;
    private Vector3 stereoCameraPos = Vector3.zero;
    private CameraClearFlags stereoCameraClearFlags = CameraClearFlags.Skybox;
    private Color stereoCameraBackgroundColor = Color.blue;

    // Blitter game object.
    protected GameObject blitter;

    /// <summary>
    /// Encoder settings for video encoding.
    /// </summary>
    [Header("Encoder Settings")]
    [SerializeField]
    public EncoderPreset encoderPreset = EncoderPreset.H264_MP4;

    [SerializeField]
    public bool enableInternalSettings = false;

    // The final output texture
    protected RenderTexture outputTexture = null;
    // The stereo video target texture
    protected RenderTexture stereoTexture = null;
    // The stereo video output texture
    protected RenderTexture stereoOutputTexture = null;
    // The equirectangular video target texture
    protected RenderTexture equirectTexture = null;
    // The stereo equirectangular video target texture
    protected RenderTexture stereoEquirectTexture = null;
    // The cubemap video target texture
    protected RenderTexture cubemapTexture = null;
    // The cubemap video render target
    protected RenderTexture cubemapRenderTarget = null;

    // The material for processing equirectangular video
    protected Material equirectMaterial;
    // The material for processing cubemap video
    protected Material cubemapMaterial;
    // The material for processing stereo video
    protected Material stereoPackMaterial;

    // Output image frame width
    public Int32 outputFrameWidth { get; protected set; }
    // Output image frame height
    public Int32 outputFrameHeight { get; protected set; }

    [Tooltip("Offset spherical coordinates (shift equirect)")]
    protected Vector2 sphereOffset = new Vector2(0, 1);
    [Tooltip("Scale spherical coordinates (flip equirect, usually just 1 or -1)")]
    protected Vector2 sphereScale = new Vector2(1, -1);

    // The audio recorder
    protected IRecorder audioRecorder;

    // The last video save file
    protected string lastVideoFile;

    // The ffmpeg full path
    protected string ffmpegFullPath;

    // The save folder full path
    protected string saveFolderFullPath = "";

    // The custom video file name
    protected string customFileName = null;

    // Log message format template
    protected string LOG_FORMAT = "[VideoCapture] {0}";

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

    protected void EnqueueCompleteEvent(CaptureCompleteEventArgs evtArgs)
    {
      completeEventQueue.Enqueue(evtArgs);
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

    protected void EnqueueErrorEvent(CaptureErrorEventArgs evtArgs)
    {
      errorEventQueue.Enqueue(evtArgs);
    }

    #endregion

    #region Capture Base

    public void SetCustomFileName(string fileName)
    {
      customFileName = fileName;
    }

    /// <summary>
    /// Start capture session.
    /// </summary>
    public virtual bool StartCapture()
    {
      Debug.LogWarningFormat(LOG_FORMAT, "StartCapture not implemented!");
      return false;
    }

    /// <summary>
    /// Stop capture session.
    /// </summary>
    public virtual bool StopCapture()
    {
      Debug.LogWarningFormat(LOG_FORMAT, "StopCapture not implemented!");
      return false;
    }

    /// <summary>
    /// Cancel capture session.
    /// </summary>
    public virtual bool CancelCapture()
    {
      Debug.LogWarningFormat(LOG_FORMAT, "CancelCapture not implemented!");
      return false;
    }

    /// <summary>
    /// Prepare capture settings.
    /// </summary>
    protected bool PrepareCapture()
    {
      if (status != CaptureStatus.READY)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Previous capture session not finish yet!");
        OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.CAPTURE_ALREADY_IN_PROGRESS));
        return false;
      }

      if (!FFmpegConfig.IsExist())
      {
        Debug.LogErrorFormat(LOG_FORMAT,
          "FFmpeg not found, please follow document and add ffmpeg executable before start capture!");
        OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.FFMPEG_NOT_FOUND));
        return false;
      }

      if (captureSource == CaptureSource.RENDERTEXTURE)
      {
        if (inputTexture == null)
        {
          Debug.LogErrorFormat(LOG_FORMAT, "Input render texture not found, please attach input render texture!");
          OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.INPUT_TEXTURE_NOT_FOUND));
          return false;
        }
        if (captureMode != CaptureMode.REGULAR)
        {
          Debug.LogFormat(LOG_FORMAT, "Capture from render texture only support REGULAR CaptureMode");
          captureMode = CaptureMode.REGULAR;
          projectionType = ProjectionType.NONE;
        }
        if (stereoMode != StereoMode.NONE)
        {
          Debug.LogFormat(LOG_FORMAT, "Capture from render texture only support NONE StereoMode");
          stereoMode = StereoMode.NONE;
        }
        frameWidth = inputTexture.width;
        frameHeight = inputTexture.height;
      }
      else if (captureSource == CaptureSource.SCREEN)
      {
        if (captureMode != CaptureMode.REGULAR)
        {
          Debug.LogFormat(LOG_FORMAT, "Capture from screen only support REGULAR CaptureMode");
          captureMode = CaptureMode.REGULAR;
          projectionType = ProjectionType.NONE;
        }
        if (stereoMode != StereoMode.NONE)
        {
          Debug.LogFormat(LOG_FORMAT, "Capture from screen only support NONE StereoMode");
          stereoMode = StereoMode.NONE;
        }
        if (captureCursor)
        {
          Cursor.SetCursor(cursorImage, Vector2.zero, CursorMode.ForceSoftware);
        }
        frameWidth = Screen.width;
        frameHeight = Screen.height;
      }
      else
      {
        ResolutionPresetSettings();
      }
      // Some codec cannot handle odd video size
      frameWidth = Utils.GetClosestEvenNumber(frameWidth);
      frameHeight = Utils.GetClosestEvenNumber(frameHeight);

      if (captureType == CaptureType.LIVE)
      {
        if (string.IsNullOrEmpty(liveStreamUrl))
        {
          Debug.LogWarningFormat(LOG_FORMAT, "Please input a valid live streaming url.");
          OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.INVALID_STREAM_URI));
          return false;
        }
      }

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

      if (frameRate < 18)
      {
        frameRate = 18;
        Debug.LogFormat(LOG_FORMAT, "Minimum frame rate is 18, set frame rate to 18.");
      }

      if (frameRate > 120)
      {
        frameRate = 120;
        Debug.LogFormat(LOG_FORMAT, "Maximum frame rate is 120, set frame rate to 120.");
      }

      AntiAliasingSettings();

      if (captureAudio && offlineRender)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture not supported in offline render mode, disable audio capture!");
        captureAudio = false;
      }

      // Save camera settings
      SaveCameraSettings();

      if (transparent)
      {
        TransparentCameraSettings();
      }

      ffmpegFullPath = FFmpegConfig.path;
      saveFolderFullPath = Utils.CreateFolder(saveFolder);
      lastVideoFile = "";

      return true;
    }

    /// <summary>
    /// Settings for 360 capture cubemap size.
    /// </summary>
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
      else if (cubemapFaceSize == CubemapFaceSize._4096)
      {
        cubemapSize = 4096;
      }
      else if (cubemapFaceSize == CubemapFaceSize._8192)
      {
        cubemapSize = 8192;
      }
    }

    /// <summary>
    /// Settings for capture anti aliasing.
    /// </summary>
    private void AntiAliasingSettings()
    {
      if (antiAliasing == AntiAliasingSetting._1)
      {
        antiAliasingSetting = 1;
      }
      else if (antiAliasing == AntiAliasingSetting._2)
      {
        antiAliasingSetting = 2;
      }
      else if (antiAliasing == AntiAliasingSetting._4)
      {
        antiAliasingSetting = 4;
      }
      else if (antiAliasing == AntiAliasingSetting._8)
      {
        antiAliasingSetting = 8;
      }
    }

    /// <summary>
    /// Settings for capture resolution.
    /// </summary>
    private void ResolutionPresetSettings()
    {
      if (resolutionPreset == ResolutionPreset._1280x720)
      {
        frameWidth = 1280;
        frameHeight = 720;
        bitrate = 2000;
      }
      else if (resolutionPreset == ResolutionPreset._1920x1080)
      {
        frameWidth = 1920;
        frameHeight = 1080;
        bitrate = 4000;
      }
      else if (resolutionPreset == ResolutionPreset._2048x1024)
      {
        frameWidth = 2048;
        frameHeight = 1024;
        bitrate = 4000;
      }
      else if (resolutionPreset == ResolutionPreset._2560x1280)
      {
        frameWidth = 2560;
        frameHeight = 1280;
        bitrate = 6000;
      }
      else if (resolutionPreset == ResolutionPreset._2560x1440)
      {
        frameWidth = 2560;
        frameHeight = 1440;
        bitrate = 6000;
      }
      else if (resolutionPreset == ResolutionPreset._3840x1920)
      {
        frameWidth = 3840;
        frameHeight = 1920;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._3840x2160)
      {
        frameWidth = 3840;
        frameHeight = 2160;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._4096x2048)
      {
        frameWidth = 4096;
        frameHeight = 2048;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._4096x2160)
      {
        frameWidth = 4096;
        frameHeight = 2160;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._7680x3840)
      {
        frameWidth = 7680;
        frameHeight = 3840;
        bitrate = 25000;
      }
      else if (resolutionPreset == ResolutionPreset._7680x4320)
      {
        frameWidth = 7680;
        frameHeight = 4320;
        bitrate = 25000;
      }
      else if (resolutionPreset == ResolutionPreset._15360x8640)
      {
        frameWidth = 15360;
        frameHeight = 8640;
        bitrate = 50000;
      }
      else if (resolutionPreset == ResolutionPreset._16384x8192)
      {
        frameWidth = 16384;
        frameHeight = 8192;
        bitrate = 50000;
      }
      else if (resolutionPreset == ResolutionPreset.CUSTOM)
      {
        if (frameWidth % 2 == 1)
        {
          frameWidth -= 1;
        }
        if (frameHeight % 2 == 1)
        {
          frameHeight -= 1;
        }
      }
    }

    /// <summary>
    /// Save camera position before capture.
    /// </summary>
    public void SaveCameraSettings()
    {
      if (captureSource == CaptureSource.CAMERA)
      {
        // regular camera
        regularCameraPos = regularCamera.transform.localPosition;
        regularCameraClearFlags = regularCamera.clearFlags;
        regularCameraBackgroundColor = regularCamera.backgroundColor;
        // stereo camera
        stereoCameraPos = stereoCamera.transform.localPosition;
        stereoCameraClearFlags = stereoCamera.clearFlags;
        stereoCameraBackgroundColor = stereoCamera.backgroundColor;
      }
    }

    /// <summary>
    /// Set transparent camera capture settings.
    /// </summary>
    public void TransparentCameraSettings()
    {
      if (captureSource == CaptureSource.CAMERA)
      {
        // regular camera
        regularCamera.clearFlags = CameraClearFlags.SolidColor;
        regularCamera.backgroundColor = new Color(0, 0, 0, 0);
        // stereo camera
        stereoCamera.clearFlags = CameraClearFlags.SolidColor;
        stereoCamera.backgroundColor = new Color(0, 0, 0, 0);
      }
    }

    /// <summary>
    /// Reset camera positions after stereo capture.
    /// </summary>
    public void ResetCameraSettings()
    {
      if (captureSource == CaptureSource.CAMERA)
      {
        // regular camera
        regularCamera.transform.localPosition = regularCameraPos;
        regularCamera.clearFlags = regularCameraClearFlags;
        regularCamera.backgroundColor = regularCameraBackgroundColor;
        regularCamera.targetTexture = null;
        // stereo camera
        stereoCamera.transform.localPosition = stereoCameraPos;
        stereoCamera.clearFlags = stereoCameraClearFlags;
        stereoCamera.backgroundColor = stereoCameraBackgroundColor;
        stereoCamera.targetTexture = null;
      }
    }

    /// <summary>
    /// Create a blitter object to keep frames presented on the screen
    /// </summary>
    protected void CreateBlitterInstance()
    {
      if (blitter == null)
      {
        blitter = Blitter.CreateInstance(regularCamera);
      }
    }

    /// <summary>
    /// Destroy the blitter game object.
    /// </summary>
    protected void ClearBlitterInstance()
    {
      if (blitter != null)
      {
        Destroy(blitter);
        blitter = null;
      }
    }

    /// <summary>
    /// Create the RenderTexture for encoding texture
    /// </summary>
    protected void CreateRenderTextures()
    {
      outputFrameWidth = frameWidth;
      outputFrameHeight = frameHeight;

      if (outputTexture != null &&
        (outputFrameWidth != outputTexture.width ||
        outputFrameHeight != outputTexture.height))
      {
        ClearRenderTextures();
      }

      // Capture from screen
      if (captureSource == CaptureSource.SCREEN)
        return;

      // Capture from user input render texture
      if (captureSource == CaptureSource.RENDERTEXTURE)
        return;

      // Create a RenderTexture with desired frame size for dedicated camera capture to store pixels in GPU.
      outputTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasingSetting, outputTexture);
      // For capturing normal 2D video, use frameTexture(Texture2D) for intermediate cpu saving, frameRenderTexture(RenderTexture) store the pixels read by frameTexture.
      regularCamera.targetTexture = outputTexture;
      // For capture panorama video:
      // EQUIRECTANGULAR: use cubemapTexture(RenderTexture) for intermediate cpu saving.
      // CUBEMAP: use texture2D for intermediate cpu saving.
      if (captureMode == CaptureMode._360)
      {
        if (projectionType == ProjectionType.CUBEMAP)
        {
          CreateCubemapTextures();
        }
        else
        {
          // Create equirectangular textures and materials.
          CreateEquirectTextures();
        }
      }
    }

    /// <summary>
    /// Clear the RenderTexture for encoding texture
    /// </summary>
    protected void ClearRenderTextures()
    {
      if (outputTexture != null)
      {
        if (Application.isEditor)
        {
          DestroyImmediate(outputTexture);
        }
        else
        {
          Destroy(outputTexture);
        }
        outputTexture = null;
      }

      if (equirectTexture != null)
      {
        if (Application.isEditor)
        {
          DestroyImmediate(equirectTexture);
        }
        else
        {
          Destroy(equirectTexture);
        }
        equirectTexture = null;
      }

      if (cubemapTexture != null)
      {
        if (Application.isEditor)
        {
          DestroyImmediate(cubemapTexture);
        }
        else
        {
          Destroy(cubemapTexture);
        }
        cubemapTexture = null;
      }

      if (stereoEquirectTexture != null)
      {
        if (Application.isEditor)
        {
          DestroyImmediate(stereoEquirectTexture);
        }
        else
        {
          Destroy(stereoEquirectTexture);
        }
        stereoEquirectTexture = null;
      }

      if (stereoTexture != null)
      {
        if (Application.isEditor)
        {
          DestroyImmediate(stereoTexture);
        }
        else
        {
          Destroy(stereoTexture);
        }
        stereoTexture = null;
      }

      if (stereoOutputTexture != null)
      {
        if (Application.isEditor)
        {
          DestroyImmediate(stereoOutputTexture);
        }
        else
        {
          Destroy(stereoOutputTexture);
        }
        stereoOutputTexture = null;
      }
    }

    /// <summary>
    /// Texture settings for stereo video capture.
    /// </summary>
    protected void CreateStereoTextures()
    {
      if (captureSource == CaptureSource.CAMERA && stereoMode != StereoMode.NONE)
      {
        // Stereo camera settings.
        regularCamera.transform.Translate(new Vector3(-interpupillaryDistance / 2, 0, 0), Space.Self);
        stereoCamera.transform.Translate(new Vector3(interpupillaryDistance / 2, 0, 0), Space.Self);

        // Init stereo video material.
        stereoPackMaterial = Utils.CreateMaterial("VideoCapture/StereoPack", stereoPackMaterial);
        stereoPackMaterial.DisableKeyword("STEREOPACK_TOP");
        stereoPackMaterial.DisableKeyword("STEREOPACK_BOTTOM");
        stereoPackMaterial.DisableKeyword("STEREOPACK_LEFT");
        stereoPackMaterial.DisableKeyword("STEREOPACK_RIGHT");

        // Init the temporary stereo target texture.
        stereoTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasingSetting, stereoTexture);
        // Set stereo camera
        //if (captureMode == CaptureMode.REGULAR)
        //  stereoCamera.targetTexture = stereoTexture;
        //else
        //  stereoCamera.targetTexture = null;
        stereoCamera.targetTexture = stereoTexture;

        // Init the final stereo texture.
        stereoOutputTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasingSetting, stereoOutputTexture);
      }
    }

    /// <summary>
    /// Texture settings for equirectangular capture.
    /// </summary>
    protected void CreateEquirectTextures()
    {
      if (captureMode == CaptureMode._360 && projectionType == ProjectionType.EQUIRECT)
      {
        // Create material for convert cubemap to equirectangular.
        equirectMaterial = Utils.CreateMaterial("VideoCapture/CubemapToEquirect", equirectMaterial);

        // Create equirectangular render texture.
        equirectTexture = Utils.CreateRenderTexture(cubemapSize, cubemapSize, 24, antiAliasingSetting, equirectTexture, false);
        equirectTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;

        if (stereoMode != StereoMode.NONE)
        {
          // Create stereo equirectangular render texture.
          stereoEquirectTexture = Utils.CreateRenderTexture(cubemapSize, cubemapSize, 24, antiAliasingSetting, stereoEquirectTexture, false);
          stereoEquirectTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        }
      }
    }

    /// <summary>
    /// Texture settings for cubemap capture.
    /// </summary>
    protected void CreateCubemapTextures()
    {
      // Create cubemap render target.
      cubemapRenderTarget = Utils.CreateRenderTexture(outputFrameWidth, outputFrameWidth, 0, antiAliasingSetting, cubemapRenderTarget);
      cubemapMaterial = Utils.CreateMaterial("VideoCapture/CubemapDisplay", cubemapMaterial);

      // Create cubemap render texture
      if (cubemapTexture == null)
      {
        cubemapTexture = new RenderTexture(cubemapSize, cubemapSize, 0);
        cubemapTexture.hideFlags = HideFlags.HideAndDontSave;
#if UNITY_5_4_OR_NEWER
        cubemapTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
#else
        cubemapTexture.isCubemap = true;
#endif
      }
    }

    /// <summary>
    /// Conversion video format to stereo.
    /// </summary>
    protected void BlitStereoTextures()
    {
      if (stereoMode != StereoMode.NONE)
      {
        // Left eye
        if (stereoMode == StereoMode.TOP_BOTTOM)
        {
          stereoPackMaterial.DisableKeyword("STEREOPACK_BOTTOM");
          stereoPackMaterial.EnableKeyword("STEREOPACK_TOP");
        }
        else if (stereoMode == StereoMode.LEFT_RIGHT)
        {
          stereoPackMaterial.DisableKeyword("STEREOPACK_RIGHT");
          stereoPackMaterial.EnableKeyword("STEREOPACK_LEFT");
        }

        Graphics.Blit(outputTexture, stereoOutputTexture, stereoPackMaterial);

        // Right eye
        if (stereoMode == StereoMode.TOP_BOTTOM)
        {
          stereoPackMaterial.EnableKeyword("STEREOPACK_BOTTOM");
          stereoPackMaterial.DisableKeyword("STEREOPACK_TOP");
        }
        else if (stereoMode == StereoMode.LEFT_RIGHT)
        {
          stereoPackMaterial.EnableKeyword("STEREOPACK_RIGHT");
          stereoPackMaterial.DisableKeyword("STEREOPACK_LEFT");
        }

        Graphics.Blit(stereoTexture, stereoOutputTexture, stereoPackMaterial);
      }
    }

    protected void RenderCubeFace(CubemapFace face, float x, float y, float w, float h)
    {
      // Texture coordinates for displaying each cube map face
      Vector3[] faceTexCoords =
      {
        // +x
        new Vector3(1, 1, 1),
        new Vector3(1, -1, 1),
        new Vector3(1, -1, -1),
        new Vector3(1, 1, -1),
        // -x
        new Vector3(-1, 1, -1),
        new Vector3(-1, -1, -1),
        new Vector3(-1, -1, 1),
        new Vector3(-1, 1, 1),

        // -y
        new Vector3(-1, -1, 1),
        new Vector3(-1, -1, -1),
        new Vector3(1, -1, -1),
        new Vector3(1, -1, 1),
        // +y flipped with -y
        new Vector3(-1, 1, -1),
        new Vector3(-1, 1, 1),
        new Vector3(1, 1, 1),
        new Vector3(1, 1, -1),

        // +z
        new Vector3(-1, 1, 1),
        new Vector3(-1, -1, 1),
        new Vector3(1, -1, 1),
        new Vector3(1, 1, 1),
        // -z
        new Vector3(1, 1, -1),
        new Vector3(1, -1, -1),
        new Vector3(-1, -1, -1),
        new Vector3(-1, 1, -1),
      };

      GL.PushMatrix();
      GL.LoadOrtho();
      GL.LoadIdentity();

      int i = (int)face;

      GL.Begin(GL.QUADS);
      GL.TexCoord(faceTexCoords[i * 4]);
      GL.Vertex3(x, y, 0);
      GL.TexCoord(faceTexCoords[i * 4 + 1]);
      GL.Vertex3(x, y + h, 0);
      GL.TexCoord(faceTexCoords[i * 4 + 2]);
      GL.Vertex3(x + w, y + h, 0);
      GL.TexCoord(faceTexCoords[i * 4 + 3]);
      GL.Vertex3(x + w, y, 0);
      GL.End();

      GL.PopMatrix();
    }

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected void Awake()
    {
      status = CaptureStatus.READY;

      if (startOnAwake)
      {
        StartCapture();
      }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    protected void Update()
    {
      if (startOnAwake)
      {
        if (Time.time >= captureTime && status == CaptureStatus.STARTED)
        {
          StopCapture();
        }
        if (quitAfterCapture && status == CaptureStatus.READY)
        {
#if UNITY_EDITOR
          UnityEditor.EditorApplication.isPlaying = false;
#else
          Application.Quit();
#endif
        }
      }

      while (completeEventQueue.Count > 0)
      {
        CaptureCompleteEventArgs args = completeEventQueue.Dequeue();
        OnCaptureComplete(args);
      }

      while (errorEventQueue.Count > 0)
      {
        CaptureErrorEventArgs args = errorEventQueue.Dequeue();
        OnCaptureError(args);
      }

      if (!string.IsNullOrEmpty(lastVideoFile))
      {
        // Save last recorded video file
        VideoCache.CacheLastVideoFile(lastVideoFile);
        lastVideoFile = "";
      }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    protected void OnDestroy()
    {
      // Check if still processing on destroy
      if (status == CaptureStatus.STARTED)
      {
        StopCapture();
      }

      ClearRenderTextures();

      ClearBlitterInstance();
    }

    #endregion
  }
}