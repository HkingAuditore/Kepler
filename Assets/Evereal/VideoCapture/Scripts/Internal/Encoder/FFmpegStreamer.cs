/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Evereal.VideoCapture
{
  // This script will merge video/audio to flv and publish to remote streaming service.
  public class FFmpegStreamer : MonoBehaviour
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
      string audioPath,
      string ffmpegPath,
      bool isLive);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanMuxingProcess(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartLiveStreaming(
      string streamUrl,
      string ffmpegPath);

    [DllImport("FFmpegEncoder")]
    private static extern bool FFmpegEncoder_SendLiveVideo(IntPtr api, string file);

    [DllImport("FFmpegEncoder")]
    private static extern bool FFmpegEncoder_StopLiveStreaming(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanLiveStreaming(IntPtr api);

    #endregion

    #region Properties

    public static FFmpegStreamer singleton;

    // Live stream url
    public string liveStreamUrl = "";
    // Video bitrate
    public Int32 bitrate = 2000;

    // Live stream is already started
    public bool streamStarted { get; private set; }
    // If live stream with audio
    public bool captureAudio { get; set; }

    // Video slice for live streaming.
    private Queue<string> videoSliceQueue;
    // Audio slice for live streaming.
    private Queue<string> audioSliceQueue;
    // Flv live streaming video slice.
    private Queue<string> liveSliceQueue;
    // Pending delete file queue
    private Queue<string> deleteSliceQueue;

    // The audio/video mux thread.
    private Thread muxingThread;

    // The live streaming thread.
    private Thread streamThread;

    AutoResetEvent muxReady = new AutoResetEvent(false);

    // The ffmpeg executable path.
    public string ffmpegFullPath { get; set; }

    // Reference to native encoder API
    private IntPtr nativeAPI;

    // Log message format template
    private string LOG_FORMAT = "[FFmpegStreamer] {0}";

    #endregion

    #region Live Streaming

    public bool StartStream()
    {
      // Check if we can start stream session
      if (streamStarted)
        return false;

      // Check if live stream url is correct set
      if (string.IsNullOrEmpty(liveStreamUrl))
        return false;

      // Reset slice queue
      videoSliceQueue = new Queue<string>();
      audioSliceQueue = new Queue<string>();
      liveSliceQueue = new Queue<string>();
      deleteSliceQueue = new Queue<string>();

      if (streamThread != null && streamThread.IsAlive)
      {
        streamThread.Abort();
        streamThread = null;
      }

      streamStarted = true;

      nativeAPI = FFmpegEncoder_StartLiveStreaming(
        liveStreamUrl,
        ffmpegFullPath);

      if (nativeAPI == IntPtr.Zero)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "Start live streaming failed!");
        return false;
      }

      if (captureAudio)
      {
        // Start video/audio thread.
        if (muxingThread != null)
        {
          if (muxingThread.IsAlive)
            muxingThread.Abort();
          muxingThread = null;
        }
        muxingThread = new Thread(MuxingThreadFunction);
        muxingThread.Priority = System.Threading.ThreadPriority.Normal;
        //muxingThread.IsBackground = true;
        muxingThread.Start();
      }

      // Start live stream thread.
      if (streamThread != null)
      {
        if (streamThread.IsAlive)
          streamThread.Abort();
        streamThread = null;
      }
      streamThread = new Thread(LiveStreamThreadProcess);
      streamThread.Priority = System.Threading.ThreadPriority.Normal;
      //streamThread.IsBackground = true;
      streamThread.Start();

      return true;
    }

    public bool StopStream()
    {
      if (!streamStarted)
        return false;

      streamStarted = false;

      return true;
    }

    public void EnqueueVideoSlice(string slice)
    {
      if (captureAudio)
      {
        lock (videoSliceQueue)
        {
          videoSliceQueue.Enqueue(slice);
        }
      }
      else
      {
        lock (liveSliceQueue)
        {
          liveSliceQueue.Enqueue(slice);
          muxReady.Set();
        }
      }
    }

    public void EnqueueAudioSlice(string slice)
    {
      lock (audioSliceQueue)
      {
        audioSliceQueue.Enqueue(slice);
      }
    }

    // Muxing video/audio in thread
    private void MuxingThreadFunction()
    {
      while (streamStarted || (videoSliceQueue.Count > 0 && audioSliceQueue.Count > 0))
      {
        if (videoSliceQueue.Count > 0 && audioSliceQueue.Count > 0)
        {
          string videoSlice;
          string audioSlice;
          lock (videoSliceQueue)
          {
            videoSlice = videoSliceQueue.Dequeue();
          }
          lock (audioSliceQueue)
          {
            audioSlice = audioSliceQueue.Dequeue();
          }
          string ext = Path.GetExtension(videoSlice);
          string liveSlice = videoSlice.Replace(ext, ".flv");

          int waitCount = 0;
          while (!File.Exists(liveSlice) && waitCount++ < 10)
          {
            Thread.Sleep(1000);
            IntPtr muxingAPI = FFmpegEncoder_StartMuxingProcess(
              EncoderPreset.H264_MP4,
              bitrate,
              false,
              false,
              liveSlice,
              videoSlice,
              audioSlice,
              ffmpegFullPath,
              true);
            if (muxingAPI == IntPtr.Zero)
              return;
            FFmpegEncoder_CleanMuxingProcess(muxingAPI);
          }

          if (waitCount >= 10)
            return;

          if (streamStarted)
          {
            lock (liveSliceQueue)
            {
              liveSliceQueue.Enqueue(liveSlice);
              muxReady.Set();
            }
          }
          else
          {
            if (File.Exists(liveSlice))
              File.Delete(liveSlice);
          }

          // Clean temp file
          if (File.Exists(videoSlice))
            File.Delete(videoSlice);
          if (File.Exists(audioSlice))
            File.Delete(audioSlice);
        }
      }
    }

    // Live streaming process in thread
    private void LiveStreamThreadProcess()
    {
      while (streamStarted || liveSliceQueue.Count > 0)
      {
        if (liveSliceQueue.Count > 0)
        {
          string liveSlice;
          lock (liveSliceQueue)
          {
            liveSlice = liveSliceQueue.Dequeue();
          }
          if (!FFmpegEncoder_SendLiveVideo(nativeAPI, liveSlice))
          {
            Debug.LogWarningFormat(LOG_FORMAT, "Failed to send live stream video file!");
          }

          while (deleteSliceQueue.Count > 0)
          {
            string deleteSlice = deleteSliceQueue.Dequeue();
            // Clean live video file
            if (File.Exists(deleteSlice))
              File.Delete(deleteSlice);
          }

          deleteSliceQueue.Enqueue(liveSlice);
        }
        else if (streamStarted)
        {
          // Wait new captured frames
          muxReady.WaitOne();
        }
        else
        {
          break;
        }
      }

      FFmpegEncoder_StopLiveStreaming(nativeAPI);
      FFmpegEncoder_CleanLiveStreaming(nativeAPI);

      // Clean all temp file
      while (videoSliceQueue.Count > 0)
      {
        string videoSlice;
        lock (videoSliceQueue)
        {
          videoSlice = videoSliceQueue.Dequeue();
        }
        if (File.Exists(videoSlice))
          File.Delete(videoSlice);
      }
      while (audioSliceQueue.Count > 0)
      {
        string audioSlice;
        lock (audioSliceQueue)
        {
          audioSlice = audioSliceQueue.Dequeue();
        }
        if (File.Exists(audioSlice))
          File.Delete(audioSlice);
      }
      while (deleteSliceQueue.Count > 0)
      {
        string deleteSlice;
        lock (deleteSliceQueue)
        {
          deleteSlice = deleteSliceQueue.Dequeue();
        }
        if (File.Exists(deleteSlice))
          File.Delete(deleteSlice);
      }
    }

    private void Awake()
    {
      if (singleton != null)
        return;
      singleton = this;

      streamStarted = false;
    }

    private void OnDestroy()
    {
      if (streamStarted)
      {
        StopStream();
      }
    }

    #endregion
  }
}