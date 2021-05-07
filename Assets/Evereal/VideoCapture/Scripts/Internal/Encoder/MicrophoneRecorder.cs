/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Evereal.VideoCapture
{
  // This script will record audio file from target microphone device.
  public class MicrophoneRecorder : MonoBehaviour, IRecorder
  {
    #region Properties

    // MicrophoneRecorder singleton
    public static MicrophoneRecorder singleton;

    // If set live streaming mode, will encoded slice audio files.
    public CaptureType captureType = CaptureType.VOD;

    // Event delegate callback for complete.
    public delegate void OnCompleteEvent(string savePath);
    // Event delegate callback for error.
    public delegate void OnErrorEvent(EncoderErrorCode error);
    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // Is audio record started
    public bool recordStarted { get; private set; }
    // Record sample rate
    private int outputSampleRate;

    // The captured audio path
    public string audioSavePath;
    // Audio slice for live streaming.
    private string audioSlicePath;

    // The save folder path for recorded files
    public string saveFolderFullPath { get; set; }

    // Microphone device index
    public int deviceIndex { get; set; }
    // Microphone device name
    private string deviceName;

    // Audio source for capture microphone
    private AudioSource audioSource;

    // Temporary audio vector we write to every second while recording is enabled
    List<float> tempRecording;

    // Log message format template
    private string LOG_FORMAT = "[MicrophoneRecorder] {0}";

    #endregion

    #region Microphone Recorder

    // If record started
    public bool RecordStarted()
    {
      return recordStarted;
    }

    // Start capture microphone session
    public bool StartRecord()
    {
#if !UNITY_WEBGL
      // Check if we can start capture session
      if (recordStarted)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS);
        return false;
      }

      // Init audio save destination
      if (captureType == CaptureType.VOD)
      {
        audioSavePath = string.Format("{0}audio_{1}_{2}.wav",
          saveFolderFullPath,
          Utils.GetTimeString(),
          Utils.GetRandomString(5));
      }
      else if (captureType == CaptureType.LIVE)
      {
        audioSlicePath = string.Format("{0}{1}.wav",
          saveFolderFullPath,
          Utils.GetTimestampString());
      }

      audioSource = GetComponent<AudioSource>();
      if (audioSource == null)
      {
        audioSource = gameObject.AddComponent<AudioSource>();
      }

      if (Microphone.devices.Length == 0)
      {
        OnError(EncoderErrorCode.MIC_DEVICE_NOT_FOUND);
        return false;
      }

      tempRecording = new List<float>();
      outputSampleRate = AudioSettings.outputSampleRate;

      recordStarted = true;

      deviceName = null;
      if (deviceIndex < Microphone.devices.Length)
      {
        deviceName = Microphone.devices[deviceIndex];
      }

      // set up recording to last a max of {MIC_AUDIO_INTERVAL_SECONDS} seconds and loop over and over
      if (captureType == CaptureType.VOD)
      {
        audioSource.clip = Microphone.Start(deviceName, true, Constants.MIC_AUDIO_INTERVAL_SECONDS, outputSampleRate);
        // resize our temporary vector every {MIC_AUDIO_INTERVAL_SECONDS} seconds
        Invoke("ResizeRecording", Constants.MIC_AUDIO_INTERVAL_SECONDS);
      }
      else if (captureType == CaptureType.LIVE)
      {
        audioSource.clip = Microphone.Start(deviceName, true, Constants.LIVE_VIDEO_SLICE_SECONDS, outputSampleRate);
        // resize our temporary vector every {LIVE_VIDEO_SLICE_SECONDS} seconds
        Invoke("ResizeRecording", Constants.LIVE_VIDEO_SLICE_SECONDS);
      }

      return true;
#else
      return false;
#endif
    }

    public bool StopRecord()
    {
#if !UNITY_WEBGL
      if (!recordStarted)
      {
        Debug.LogFormat(LOG_FORMAT, "Microphone record session not start yet!");
        return false;
      }

      recordStarted = false;

      // stop recording, get length, create a new array of samples
      int length = Microphone.GetPosition(deviceName);

      Microphone.End(deviceName);

      if (captureType == CaptureType.VOD)
      {
        float[] clipData = new float[length];
        audioSource.clip.GetData(clipData, 0);

        // create a larger vector that will have enough space to hold our temporary
        // recording, and the last section of the current recording
        float[] fullClip = new float[clipData.Length + tempRecording.Count];
        for (int i = 0; i < fullClip.Length; i++)
        {
          // write data all recorded data to fullCLip vector
          if (i < tempRecording.Count)
            fullClip[i] = tempRecording[i];
          else
            fullClip[i] = clipData[i - tempRecording.Count];
        }

        AudioClip clip = AudioClip.Create("recorded samples", fullClip.Length, 1, outputSampleRate, false);
        clip.SetData(fullClip, 0);

        WavEncoder.Save(audioSavePath, clip);

        OnComplete(audioSavePath);
      }

      tempRecording.Clear();
      tempRecording = null;

      return true;
#else
      return false;
#endif
    }

    public bool CancelRecord()
    {
#if !UNITY_WEBGL
      if (!recordStarted)
      {
        Debug.LogFormat(LOG_FORMAT, "Microphone record session not start yet!");
        return false;
      }

      recordStarted = false;

      Microphone.End(deviceName);

      tempRecording.Clear();
      tempRecording = null;

      if (File.Exists(audioSavePath))
        File.Delete(audioSavePath);
      audioSavePath = "";
      if (File.Exists(audioSlicePath))
        File.Delete(audioSlicePath);
      audioSlicePath = "";

      return true;
#else
      return false;
#endif
    }

    public string GetRecordedAudio()
    {
      return audioSavePath;
    }

    private void ResizeRecording()
    {
      if (recordStarted)
      {
        if (captureType == CaptureType.VOD)
        {
          // add the next second of recorded audio to temp vector
          int length = outputSampleRate * Constants.MIC_AUDIO_INTERVAL_SECONDS;
          float[] clipData = new float[length];
          audioSource.clip.GetData(clipData, 0);
          tempRecording.AddRange(clipData);
          Invoke("ResizeRecording", Constants.MIC_AUDIO_INTERVAL_SECONDS);
        }
        else if (captureType == CaptureType.LIVE)
        {
          int length = outputSampleRate * Constants.LIVE_VIDEO_SLICE_SECONDS;
          float[] clipData = new float[length];
          audioSource.clip.GetData(clipData, 0);
          AudioClip clip = AudioClip.Create("recorded samples", clipData.Length, 1, outputSampleRate, false);
          clip.SetData(clipData, 0);
          WavEncoder.Save(audioSlicePath, clip);
          FFmpegStreamer.singleton.EnqueueAudioSlice(audioSlicePath);
          // restart
          audioSlicePath = string.Format("{0}{1}.wav",
            saveFolderFullPath,
            Utils.GetTimestampString());
          Invoke("ResizeRecording", Constants.LIVE_VIDEO_SLICE_SECONDS);
        }
      }
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
      if (singleton != null)
        return;
      singleton = this;

      recordStarted = false;
    }

    private void OnDestroy()
    {
      if (recordStarted)
      {
        StopRecord();
      }
    }

    #endregion
  }
}