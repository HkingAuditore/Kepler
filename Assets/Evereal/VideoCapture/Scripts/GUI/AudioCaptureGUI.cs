/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(AudioCapture))]
  public class AudioCaptureGUI : MonoBehaviour
  {
    private AudioCapture audioCapture;

    private void Awake()
    {
      audioCapture = GetComponent<AudioCapture>();
      Application.runInBackground = true;
    }

    private void OnEnable()
    {
      audioCapture.OnComplete += HandleCaptureComplete;
      audioCapture.OnError += HandleCaptureError;
    }

    private void OnDisable()
    {
      audioCapture.OnComplete -= HandleCaptureComplete;
      audioCapture.OnError -= HandleCaptureError;
    }

    private void HandleCaptureComplete(object sender, CaptureCompleteEventArgs args)
    {
      UnityEngine.Debug.Log("Save file to: " + args.SavePath);
    }

    private void HandleCaptureError(object sender, CaptureErrorEventArgs args)
    {
      //UnityEngine.Debug.Log(args.ErrorCode);
    }

    private void OnGUI()
    {
      if (audioCapture.status == CaptureStatus.STARTED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
        {
          audioCapture.StopCapture();
        }
        if (GUI.Button(new Rect(170, Screen.height - 60, 150, 50), "Cancel Capture"))
        {
          audioCapture.CancelCapture();
        }
      }
      else
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture"))
        {
          audioCapture.StartCapture();
        }
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(audioCapture.saveFolder);
      }
    }
  }
}