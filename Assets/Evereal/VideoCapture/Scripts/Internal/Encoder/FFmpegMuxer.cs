/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Evereal.VideoCapture
{
  // This script will mux audio into video file.
  public class FFmpegMuxer : MonoBehaviour
  {
    #region Dll Import

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartMuxingProcess(
      EncoderPreset preset,
      int bitrate,
      bool verticalFlip,
      bool horizontalFlip,
      string mediaPath,
      string videoPath,
      string audioPpath,
      string ffmpegPath,
      bool isLive);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanMuxingProcess(IntPtr api);

    #endregion

    #region Properties

    public static FFmpegMuxer singleton;

    // Event delegate callback for complete.
    public delegate void OnCompleteEvent(string savePath);
    // Event delegate callback for error.
    public delegate void OnErrorEvent(EncoderErrorCode error);
    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // Is mux process initiated
    public bool muxInitiated { get; private set; }

    // The audio file will mux with.
    private string audioFile;
    // The video files is ready to mux.
    private Queue<string> videoFiles;

    // Video capture instance for muxing process
    private List<IVideoCapture> videoCaptures;

    // The audio/video mux thread.
    private Thread muxingThread;

    AutoResetEvent muxReady = new AutoResetEvent(false);

    public bool verticalFlip { get; set; }
    public bool horizontalFlip { get; set; }
    public string customFileName { get; set; }
    public string saveFolderFullPath { get; set; }
    public string ffmpegFullPath { get; set; }

    // Log message format template
    private string LOG_FORMAT = "[FFmpegMuxer] {0}";

    #endregion

    #region FFmpeg Muxer

    public void SetAudioFile(string file)
    {
      audioFile = file;
      muxReady.Set();
    }

    public void EnqueueVideoFile(string file)
    {
      lock (videoFiles)
      {
        videoFiles.Enqueue(file);
      }
      muxReady.Set();
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

    // Init mux process
    public bool InitMux()
    {
      // Check if we can start mux process
      if (muxInitiated)
        return false;

      // Start muxing thread
      if (muxingThread != null)
      {
        if (muxingThread.IsAlive)
          muxingThread.Abort();
        muxingThread = null;
      }
      muxingThread = new Thread(MuxingProcess);
      muxingThread.Priority = System.Threading.ThreadPriority.Highest;
      muxingThread.Start();

      //StartCoroutine(MuxingProcessDebug());

      return true;
    }

    /// <summary>
    /// Media muxing thread function.
    /// </summary>
    private void MuxingProcess()
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
          if (!StartMux(videoCapture))
          {
            // Skip if not success
            continue;
          }
        }
        else
        {
          muxReady.WaitOne();
        }
      }

      // Clean audio file
      if (File.Exists(audioFile))
      {
        File.Delete(audioFile);
        audioFile = null;
      }

      muxInitiated = false;
    }

    // Start video/audio muxing process, this is blocking function
    private bool StartMux(IVideoCapture videoCapture)
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
        IntPtr nativeAPI = FFmpegEncoder_StartMuxingProcess(
          encoder.encoderPreset,
          encoder.bitrate,
          verticalFlip,
          horizontalFlip,
          videoSavePath,
          encoder.videoSavePath,
          audioFile,
          ffmpegFullPath,
          false);

        if (nativeAPI == IntPtr.Zero)
        {
          OnError(EncoderErrorCode.MUXING_FAILED_TO_START);
          return false;
        }

        FFmpegEncoder_CleanMuxingProcess(nativeAPI);
      }

      if (waitCount >= 10)
      {
        return false;
      }

      // VideoCapture muxer complete callback
      videoCapture.OnMuxerComplete(videoSavePath);
      OnComplete(videoSavePath);

      // Clean video files with no sound
      if (File.Exists(encoder.videoSavePath))
      {
        File.Delete(encoder.videoSavePath);
        encoder.videoSavePath = "";
      }

      //Debug.LogFormat(LOG_FORMAT, "Muxing process finish!");

      return true;
    }

    #endregion

    #region Unity Life Cycle

    private void MuxerErrorLog(EncoderErrorCode error)
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
      OnError += MuxerErrorLog;
    }

    private void OnDisable()
    {
      OnError -= MuxerErrorLog;
    }

    #endregion
  }
}