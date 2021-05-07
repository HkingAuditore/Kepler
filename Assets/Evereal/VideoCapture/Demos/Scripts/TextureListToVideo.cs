/* Copyright (c) 2020-present Evereal. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Sample script demonstrates how to encode Texture2D list to video.
  /// </summary>
  public class TextureListToVideo : MonoBehaviour
  {
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
    private static extern bool FFmpegEncoder_StopVodCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanVodCapture(IntPtr api);

    private IntPtr nativeAPI;
    private string videoPath;
    private List<Texture2D> textureList = new List<Texture2D>();
    private int index = 0;

    private string LOG_FORMAT = "[TextureListToVideo] {0}";

    // Use this for initialization
    void Start()
    {
      int width = 1280;
      int height = 720;
      EncoderPreset ep = EncoderPreset.H264_MP4;

      Texture2D dummyTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
      // init texture list
      for (int i = 0; i < 120; i++)
      {
        textureList.Add(dummyTexture);
      }

      videoPath = string.Format(
        "{0}video_{1}x{2}_{3}_{4}.{5}",
        Utils.CreateFolder("Captures"),
        width, height,
        Utils.GetTimeString(),
        Utils.GetRandomString(5),
        Utils.GetEncoderPresetExt(ep));

      nativeAPI = FFmpegEncoder_StartVodCapture(
        ep,
        width,
        height,
        2000,
        30,
        false,
        false,
        ProjectionType.NONE,
        StereoMode.NONE,
        videoPath,
        FFmpegConfig.path);

      Debug.LogFormat(LOG_FORMAT, "Start encoding...");
    }

    // Update is called once per frame
    void Update()
    {
      if (index < textureList.Count)
      {
        Texture2D texture = textureList[index];
        byte[] buffer = texture.GetRawTextureData();
        FFmpegEncoder_CaptureVodFrames(nativeAPI, buffer, 1);
      }

      index++;

      // add one frame after encoder finish
      if (index == textureList.Count + 1)
      {
        FFmpegEncoder_StopVodCapture(nativeAPI);
        FFmpegEncoder_CleanVodCapture(nativeAPI);
        Debug.LogFormat(LOG_FORMAT, "Video saved to " + videoPath);
      }
    }
  }
}