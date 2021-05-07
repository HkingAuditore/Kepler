/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// This script will record target audio listener sample and encode to audio file, or mux audio into video file if required.
  /// </summary>
  [Serializable]
  public class AudioCapture : MonoBehaviour, ICapture
  {
    #region Properties

    // Start capture on awake if set to true.
    [SerializeField]
    public bool startOnAwake = false;
    // Quit process after capture finish.
    [SerializeField]
    public bool quitAfterCapture = false;
    // The capture duration if start capture on awake.
    [SerializeField]
    public float captureTime = 30f;

    [Tooltip("Save folder for recorded audio")]
    // Save path for recorded video including file name (c://xxx.wav)
    [SerializeField]
    public string saveFolder = "";

    // Capture microphone settings
    [SerializeField]
    public bool captureMicrophone = false;
    // Microphone device index
    [SerializeField]
    public int deviceIndex = 0;

    // The audio recorder
    private IRecorder audioRecorder;

    // Get or set the current status.
    public CaptureStatus status
    {
      get
      {
        if (audioRecorder != null && audioRecorder.RecordStarted())
          return CaptureStatus.STARTED;
        return CaptureStatus.READY;
      }
    }

    private string saveFolderFullPath = "";

    // Log message format template
    private string LOG_FORMAT = "[AudioCapture] {0}";

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

    public event EventHandler<CaptureErrorEventArgs> OnError;

    protected void OnCaptureError(CaptureErrorEventArgs args)
    {
      EventHandler<CaptureErrorEventArgs> handler = OnError;
      if (handler != null)
      {
        handler(this, args);
      }
    }

    #endregion

    #region Methods

    // Start capture audio session
    public bool StartCapture()
    {
      // Check if we can start capture session
      if (status != CaptureStatus.READY)
      {
        OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.CAPTURE_ALREADY_IN_PROGRESS));
        return false;
      }

      saveFolderFullPath = Utils.CreateFolder(saveFolder);

      // Init audio recorder
      if (captureMicrophone)
      {
        if (MicrophoneRecorder.singleton == null)
        {
          gameObject.AddComponent<MicrophoneRecorder>();
        }
        MicrophoneRecorder.singleton.saveFolderFullPath = saveFolderFullPath;
        MicrophoneRecorder.singleton.captureType = CaptureType.VOD;
        audioRecorder = MicrophoneRecorder.singleton;
      }
      else
      {
        if (AudioRecorder.singleton == null)
        {
          if (GetComponent<DontDestroy>() != null)
          {
            // Reset AudioListener
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (listener)
            {
              Destroy(listener);
              Debug.LogFormat(LOG_FORMAT, "AudioListener found, reset in game scene.");
            }
            gameObject.AddComponent<AudioListener>();
            gameObject.AddComponent<AudioRecorder>();
          }
          else
          {
            // Keep AudioListener
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (!listener)
            {
              listener = gameObject.AddComponent<AudioListener>();
              Debug.LogFormat(LOG_FORMAT, "AudioListener not found, add a new AudioListener.");
            }
            listener.gameObject.AddComponent<AudioRecorder>();
          }
        }
        AudioRecorder.singleton.saveFolderFullPath = saveFolderFullPath;
        AudioRecorder.singleton.captureType = CaptureType.VOD;
        audioRecorder = AudioRecorder.singleton;
      }

      if (audioRecorder == null)
      {
        OnCaptureError(new CaptureErrorEventArgs(CaptureErrorCode.AUDIO_CAPTURE_START_FAILED));
        return false;
      }

      audioRecorder.StartRecord();

      Debug.LogFormat(LOG_FORMAT, "Audio capture session started.");

      return true;
    }

    // Stop capture audio session
    public bool StopCapture()
    {
      if (status != CaptureStatus.STARTED)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture session not start yet!");
        return false;
      }

      audioRecorder.StopRecord();

      OnCaptureComplete(new CaptureCompleteEventArgs(audioRecorder.GetRecordedAudio()));

      Debug.LogFormat(LOG_FORMAT, "Audio capture session success!");

      return true;
    }

    // Cancel capture audio session
    public bool CancelCapture()
    {
      if (status != CaptureStatus.STARTED)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture session not start yet!");
        return false;
      }

      audioRecorder.CancelRecord();

      Debug.LogFormat(LOG_FORMAT, "Audio capture session canceled!");

      return true;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
      if (startOnAwake)
      {
        StartCapture();
      }
    }

    private void Update()
    {
      if (startOnAwake)
      {
        if (Time.time >= captureTime && status == CaptureStatus.STARTED)
        {
          StopCapture();
        }
        if (quitAfterCapture && status != CaptureStatus.STARTED)
        {
#if UNITY_EDITOR
          UnityEditor.EditorApplication.isPlaying = false;
#else
          Application.Quit();
#endif
        }
      }
    }

    private void OnDestroy()
    {
      // Check if still processing on destroy
      if (status == CaptureStatus.STARTED)
      {
        StopCapture();
      }
    }

    #endregion
  }
}