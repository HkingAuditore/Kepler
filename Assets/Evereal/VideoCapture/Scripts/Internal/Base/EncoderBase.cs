/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Base class for <c>FFmpegEncoder</c> and <c>GPUEncoder</c> class.
  /// </summary>
  public class EncoderBase : MonoBehaviour
  {
    /// <summary>
    /// Native encoder error status.
    /// </summary>
    private const int ERROR_VIDEO_ENCODING_CAUSE_ERRORS = 100;
    private const int ERROR_AUDIO_ENCODING_CAUSE_ERRORS = 200;
    private const int ERROR_TRANSCODING_MUXING_CAUSE_ERRORS = 300;
    private const int ERROR_RTMP_CAUSE_ERRORS = 400;
    private const int ERROR_GRAPHICS_CAPTURE_ERRORS = 500;
    private const int ERROR_CONFIGURATION_ERRORS = 600;
    private const int ERROR_SYSTEM_ERRORS = 700;
    private const int ERROR_ENCODING_CAPABILITY = 800;
    // TODO, cache software encoding error
    private const int ERROR_SOFTWARE_ENCODING_ERROR = 900;

    public enum EncoderStatus
    {
      // Common error codes
      OK = 0,
      ENCODE_IS_NOT_READY,
      NO_INPUT_FILE,
      FILE_READING_ERROR,
      OUTPUT_FILE_OPEN_FAILED,
      OUTPUT_FILE_CREATION_FAILED,
      DXGI_CREATING_FAILED,
      DEVICE_CREATING_FAILED,

      // Video/Image encoding specific error codes
      ENCODE_INIT_FAILED = ERROR_VIDEO_ENCODING_CAUSE_ERRORS,
      ENCODE_SET_CONFIG_FAILED,
      ENCODER_CREATION_FAILED,
      INVALID_TEXTURE_POINTER,
      CONTEXT_CREATION_FAILED,
      TEXTURE_CREATION_FAILED,
      TEXTURE_RESOURCES_COPY_FAILED,
      IO_BUFFER_ALLOCATION_FAILED,
      ENCODE_PICTURE_FAILED,
      ENCODE_FLUSH_FAILED,
      MULTIPLE_ENCODING_SESSION,
      INVALID_TEXTURE_RESOLUTION,

      // WIC specific error codes
      WIC_SAVE_IMAGE_FAILED,

      // Audio encoding specific error codes
      AUDIO_DEVICE_ENUMERATION_FAILED = ERROR_AUDIO_ENCODING_CAUSE_ERRORS,
      AUDIO_CLIENT_INIT_FAILED,
      WRITING_WAV_HEADER_FAILED,
      RELEASING_WAV_FAILED,

      // Transcoding and muxing specific error codes
      MF_CREATION_FAILED = ERROR_TRANSCODING_MUXING_CAUSE_ERRORS,
      MF_INIT_FAILED,
      MF_CREATING_WAV_FORMAT_FAILED,
      MF_TOPOLOGY_CREATION_FAILED,
      MF_TOPOLOGY_SET_FAILED,
      MF_TRANSFORM_NODE_SET_FAILED,
      MF_MEDIA_CREATION_FAILED,
      MF_HANDLING_MEDIA_SESSION_FAILED,

      // WAMEDIA muxing specific error codes
      WAMDEIA_MUXING_FAILED,

      // More MF error codes
      MF_STARTUP_FAILED,
      MF_TRANSFORM_CREATION_FAILED,
      MF_SOURCE_READER_CREATION_FAILED,
      MF_STREAM_SELECTION_FAILED,
      MF_MEDIA_TYPE_CREATION_FAILED,
      MF_MEDIA_TYPE_CONFIGURATION_FAILED,
      MF_MEDIA_TYPE_SET_FAILED,
      MF_MEDIA_TYPE_GET_FAILED,
      MF_CREATE_WAV_FORMAT_FROM_MEDIA_TYPE_FAILED,
      MF_TRANSFORM_OUTPUT_STREAM_INFO_FAILED,
      MF_CREATE_MEMORY_BUFFER_FAILED,
      MF_CREATE_SAMPLE_FAILED,
      MF_SAMPLE_ADD_BUFFER_FAILED,
      MF_READ_SAMPLE_FAILED,
      MF_TRANSFORM_FAILED,
      MF_BUFFER_LOCK_FAILED,

      // RTMP specific error codes
      INVALID_FLV_HEADER = ERROR_RTMP_CAUSE_ERRORS,
      INVALID_STREAM_URL,
      RTMP_CONNECTION_FAILED,
      RTMP_DISCONNECTED,
      SENDING_RTMP_PACKET_FAILED,

      // Graphics capture error codes
      GRAPHICS_DEVICE_CAPTURE_INIT_FAILED = ERROR_GRAPHICS_CAPTURE_ERRORS,
      GRAPHICS_DEVICE_CAPTURE_INVALID_TEXTURE,
      GRAPHICS_DEVICE_CAPTURE_OPEN_SHARED_RESOURCE_FAILED,
      GRAPHICS_DEVICE_CAPTURE_KEYED_MUTEX_ACQUIRE_FAILED,
      GRAPHICS_DEVICE_CAPTURE_KEYED_ACQUIRE_ACQUIRE_SYNC_FAILED,
      GRAPHICS_DEVICE_CAPTURE_KEYED_ACQUIRE_RELASE_SYNC_FAILED,

      // Configuration error codes
      MIC_NOT_CONFIGURED = ERROR_CONFIGURATION_ERRORS,
      MIC_REQUIRES_ENUMERATION,
      MIC_DEVICE_NOT_SET,
      MIC_ENUMERATION_FAILED,
      MIC_SET_FAILED,
      MIC_UNSET_FAILED,
      MIC_INDEX_INVALID,
      CAMERA_NOT_CONFIGURED,
      CAMERA_REQUIRES_ENUMERATION,
      CAMERA_DEVICE_NOT_SET,
      CAMERA_ENUMERATION_FAILED,
      CAMERA_SET_FAILED,
      CAMERA_UNSET_FAILED,
      CAMERA_INDEX_INVALID,
      LIVE_CAPTURE_SETTINGS_NOT_CONFIGURED,
      VOD_CAPTURE_SETTINGS_NOT_CONFIGURED,
      PREVIEW_CAPTURE_SETTINGS_NOT_CONFIGURED,

      // System error codes
      SYSTEM_INITIALIZE_FAILED = ERROR_SYSTEM_ERRORS,
      SYSTEM_ENCODING_TEXTURE_CREATION_FAILED,
      SYSTEM_PREVIEW_TEXTURE_CREATION_FAILED,
      SYSTEM_ENCODING_TEXTURE_FORMAT_CREATION_FAILED,
      SYSTEM_SCREENSHOT_TEXTURE_FORMAT_CREATION_FAILED,
      SYSTEM_CAPTURE_IN_PROGRESS,
      SYSTEM_CAPTURE_NOT_IN_PROGRESS,
      SYSTEM_CAPTURE_TEXTURE_NOT_RECEIVED,
      SYSTEM_CAMERA_OVERLAY_FAILED,
      SYSTEM_CAPTURE_PREVIEW_FAILED,
      SYSTEM_CAPTURE_PREVIEW_NOT_IN_PROGRESS,

      // Encoding capability error codes
      UNSUPPORTED_ENCODING_ENVIRONMENT = ERROR_ENCODING_CAPABILITY,
      UNSUPPORTED_GRAPHICS_CARD_DRIVER_VERSION,
      UNSUPPORTED_GRAPHICS_CARD,
      UNSUPPORTED_OS_VERSION,
      UNSUPPORTED_OS_PROCESSOR,
    }

    // Encoding preset for ffmpeg
    public EncoderPreset encoderPreset { get; set; }

    // Event delegate callback for complete.
    public delegate void OnCompleteEvent(string savePath);
    // Event delegate callback for error.
    public delegate void OnErrorEvent(EncoderErrorCode error, EncoderStatus? status);

    // You can choose capture from camera, screen or render texture.
    public CaptureSource captureSource { get; set; }
    // The type of video capture mode, regular or 360.
    public CaptureMode captureMode { get; set; }
    // The type of video capture stereo mode, left right or top bottom.
    public StereoMode stereoMode { get; set; }
    // Stereo mode settings.
    // Average IPD of all subjects in US Army survey in meters
    public float interpupillaryDistance { get; set; }
    // The type of video projection, used for 360 video capture.
    public ProjectionType projectionType { get; set; }
    // If set live streaming mode, encoded video will be push to remote streaming url instead of save to local file.
    public CaptureType captureType { get; set; }
    // Audio capture settings, set false if you want to mute audio.
    public bool captureAudio { get; set; }
    // Microphone capture settings.
    public bool captureMicrophone { get; set; }
    // Do all capture in main thread
    public bool offlineRender { get; set; }

    // Resolution preset settings, set custom for other resolutions
    public ResolutionPreset resolutionPreset { get; set; }
    // The width of video frame
    public Int32 frameWidth { get; set; }
    // The height of video frame
    public Int32 frameHeight { get; set; }
    // The size of each cubemap side
    public Int32 cubemapSize { get; set; }

    public Int32 bitrate { get; set; }
    public Int16 frameRate { get; set; }
    public Int16 antiAliasing { get; set; }
    // You can get test live stream key on "https://www.facebook.com/live/create".
    // ex. rtmp://rtmp-api-dev.facebook.com:80/rtmp/xxStreamKeyxx
    public string liveStreamUrl { get; set; }
    // User custom video file name
    public string customFileName { get; set; }
    // Save path for recorded video including file name (c://xxx.mp4)
    public string videoSavePath { get; set; }
    // Save path for screenshot including file name (c://xxx.jpg)
    public string screenshotSavePath { get; set; }
    // Save folder for capture files
    public string saveFolderFullPath { get; set; }

    ///// <summary>
    ///// Encoding setting variables for preview capture.
    ///// </summary>
    //[Header("Preview Video Settings")]
    //public ResolutionPreset previewVideoPreset = ResolutionPreset.CUSTOM;
    //public Int32 previewVideoWidth = 1280;
    //public Int32 previewVideoHeight = 720;
    //public Int32 previewVideoFramerate = 30;
    //public Int32 previewVideoBitRate = 2000;

    // Capture is already started
    public bool captureStarted { get; protected set; }
    public bool screenshotStarted { get; protected set; }

    // The camera render content will be used for capturing video.
    public Camera regularCamera { get; set; }
    public Camera stereoCamera { get; set; }

    // Blitter game object.
    protected GameObject blitter;

    // The user input texture
    public RenderTexture inputTexture { get; set; }
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

    // Output video frame width
    public Int32 outputFrameWidth { get; protected set; }
    // Output video frame height
    public Int32 outputFrameHeight { get; protected set; }

    // Offset spherical coordinates (shift equirect)
    protected Vector2 sphereOffset = new Vector2(0, 1);
    // Scale spherical coordinates (flip equirect, usually just 1 or -1)
    protected Vector2 sphereScale = new Vector2(1, -1);
    // Include camera rotation for render to cubemap
    protected bool includeCameraRotation = true;

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

      // Capture from user input render texture
      if (captureSource == CaptureSource.RENDERTEXTURE)
        return;

      // Create a RenderTexture with desired frame size for dedicated camera capture to store pixels in GPU.
      outputTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasing, outputTexture);

      // Capture from screen
      if (captureSource == CaptureSource.SCREEN)
        return;

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
        Destroy(outputTexture);
        outputTexture = null;
      }

      if (equirectTexture != null)
      {
        Destroy(equirectTexture);
        equirectTexture = null;
      }

      if (cubemapTexture != null)
      {
        Destroy(cubemapTexture);
        cubemapTexture = null;
      }

      if (stereoEquirectTexture != null)
      {
        Destroy(stereoEquirectTexture);
        stereoEquirectTexture = null;
      }

      if (stereoTexture != null)
      {
        Destroy(stereoTexture);
        stereoTexture = null;
      }

      if (stereoOutputTexture != null)
      {
        Destroy(stereoOutputTexture);
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
        stereoTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasing, stereoTexture);
        // Set stereo camera
        if (captureMode == CaptureMode.REGULAR)
          stereoCamera.targetTexture = stereoTexture;
        else
          stereoCamera.targetTexture = null;

        // Init the final stereo texture.
        stereoOutputTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasing, stereoOutputTexture);
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
        equirectTexture = Utils.CreateRenderTexture(cubemapSize, cubemapSize, 24, antiAliasing, equirectTexture, false);
        equirectTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;

        if (stereoMode != StereoMode.NONE)
        {
          // Create stereo equirectangular render texture.
          stereoEquirectTexture = Utils.CreateRenderTexture(cubemapSize, cubemapSize, 24, antiAliasing, stereoEquirectTexture, false);
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
      cubemapRenderTarget = Utils.CreateRenderTexture(outputFrameWidth, outputFrameWidth, 0, antiAliasing, cubemapRenderTarget);
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
    /// Blit texture for screen capture.
    /// </summary>
    protected void BlitScreenTextures()
    {
      Graphics.Blit(null, outputTexture);
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

    /// <summary>
    /// Render cube face.
    /// </summary>
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

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void OnDestroy()
    {
      ClearRenderTextures();
    }
  }
}