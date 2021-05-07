/* Copyright (c) 2021-present Evereal. All rights reserved. */

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Evereal.VideoCapture
{
  // This script will transcode video to different format.
  public class FFmpegTranscoder : MonoBehaviour
  {
    #region Dll Import

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartTranscodingProcess(
      EncoderPreset preset,
      int bitrate,
      bool verticalFlip,
      bool horizontalFlip,
      string mediaPath,
      string videoPath,
      string ffmpegPath);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanTranscodingProcess(IntPtr api);

    #endregion

    #region Properties

    public static FFmpegTranscoder singleton;

    // Event delegate callback for complete.
    public delegate void OnCompleteEvent(string savePath);
    // Event delegate callback for error.
    public delegate void OnErrorEvent(EncoderErrorCode error);
    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // Is transcode process initiated
    public bool transcodeInitiated { get; private set; }

    // The video files is ready to mux.
    private Queue<string> videoFiles;

    // Video capture instance for muxing process
    private List<IVideoCapture> videoCaptures;

    // The video transcode thread.
    private Thread transcodeThread;

    AutoResetEvent transcodeReady = new AutoResetEvent(false);

    public bool verticalFlip { get; set; }
    public bool horizontalFlip { get; set; }
    public string customFileName { get; set; }
    public string saveFolderFullPath { get; set; }
    public string ffmpegFullPath { get; set; }

    // Log message format template
    private string LOG_FORMAT = "[FFmpegTranscoder] {0}";

    #endregion

    #region FFmpeg Transcoder

    public void EnqueueVideoFile(string file)
    {
      lock (videoFiles)
      {
        videoFiles.Enqueue(file);
      }
      transcodeReady.Set();
    }

    public void AttachVideoCapture(IVideoCapture videoCapture)
    {
      if (!videoCaptures.Contains(videoCapture))
      {
        lock (videoCaptures)
        {
          videoCaptures.Add(videoCapture);
        }
      }
    }

    // Init transcode process
    public bool InitTranscode()
    {
      // Check if we can start transcode process
      if (transcodeInitiated)
        return false;

      // Start transcode thread
      if (transcodeThread != null)
      {
        if (transcodeThread.IsAlive)
          transcodeThread.Abort();
        transcodeThread = null;
      }
      transcodeThread = new Thread(TranscodeProcess);
      transcodeThread.Priority = System.Threading.ThreadPriority.Highest;
      transcodeThread.Start();

      return true;
    }

    /// <summary>
    /// Video transcode thread function.
    /// </summary>
    private void TranscodeProcess()
    {
      while (videoCaptures.Count > 0 || videoFiles.Count > 0)
      {
        if (videoFiles.Count > 0)
        {
          string videoFile;
          lock (videoFiles)
          {
            videoFile = videoFiles.Dequeue();
          }

          IVideoCapture videoCapture = null;
          for (int i = 0; i < videoCaptures.Count; i++)
          {
            // Check if match with attached VideoCapture
            if (videoCaptures[i].GetEncoder().videoSavePath == videoFile)
            {
              videoCapture = videoCaptures[i];
              lock (videoCaptures)
              {
                videoCaptures.RemoveAt(i);
              }
            }
          }

          if (videoCapture == null)
          {
            // Skip if no match VideoCapture
            continue;
          }

          // Start muxing process
          if (!StartTranscode(videoCapture))
          {
            // Skip if not success
            continue;
          }
        }
        else
        {
          transcodeReady.WaitOne();
        }
      }

      transcodeInitiated = false;
    }

    // Start video transcode process, this is blocking function
    private bool StartTranscode(IVideoCapture videoCapture)
    {
      EncoderBase encoder = videoCapture.GetEncoder();
      string videoSavePath = string.Format("{0}capture_{1}x{2}_{3}_{4}.{5}",
          saveFolderFullPath,
          encoder.outputFrameWidth, encoder.outputFrameHeight,
          Utils.GetTimeString(),
          Utils.GetRandomString(5),
          Utils.GetEncoderPresetExt(encoder.encoderPreset));
      if (customFileName != null)
      {
        videoSavePath = string.Format("{0}{1}.{2}",
          saveFolderFullPath,
          customFileName,
          Utils.GetEncoderPresetExt(encoder.encoderPreset));
      }

      // Make sure generated the merge file
      int waitCount = 0;
      while (!File.Exists(videoSavePath) && waitCount++ < 10)
      {
        Thread.Sleep(1000);
        IntPtr nativeAPI = FFmpegEncoder_StartTranscodingProcess(
          encoder.encoderPreset,
          encoder.bitrate,
          verticalFlip,
          horizontalFlip,
          videoSavePath,
          encoder.videoSavePath,
          ffmpegFullPath);

        if (nativeAPI == IntPtr.Zero)
        {
          OnError(EncoderErrorCode.TRANSCODE_FAILED_TO_START);
          return false;
        }

        FFmpegEncoder_CleanTranscodingProcess(nativeAPI);
      }

      if (waitCount >= 10)
      {
        return false;
      }

      // VideoCapture muxer complete callback
      videoCapture.OnTranscodeComplete(videoSavePath);
      OnComplete(videoSavePath);

      // Clean original video files
      if (File.Exists(encoder.videoSavePath))
      {
        File.Delete(encoder.videoSavePath);
        encoder.videoSavePath = "";
      }

      //Debug.LogFormat(LOG_FORMAT, "Transcode process finish!");

      return true;
    }

    #endregion

    #region Unity Life Cycle

    private void TranscodeErrorLog(EncoderErrorCode error)
    {
      Debug.LogWarningFormat(LOG_FORMAT, "Error Occured of type: " + error);
    }

    private void Awake()
    {
      if (singleton != null)
        return;
      singleton = this;

      videoCaptures = new List<IVideoCapture>();
      videoFiles = new Queue<string>();
    }

    private void OnEnable()
    {
      OnError += TranscodeErrorLog;
    }

    private void OnDisable()
    {
      OnError -= TranscodeErrorLog;
    }

    #endregion
  }
}