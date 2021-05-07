/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(VideoCapture))]
  public class VideoCaptureGUI : MonoBehaviour
  {
    private VideoCapture videoCapture;

    private void Awake()
    {
      videoCapture = GetComponent<VideoCapture>();
      Application.runInBackground = true;
    }

    private void OnEnable()
    {
      videoCapture.OnComplete += HandleCaptureComplete;
      videoCapture.OnError += HandleCaptureError;
    }

    private void OnDisable()
    {
      videoCapture.OnComplete -= HandleCaptureComplete;
      videoCapture.OnError -= HandleCaptureError;
    }

    private void HandleCaptureComplete(object sender, CaptureCompleteEventArgs args)
    {
      if (videoCapture.captureType == CaptureType.VOD)
      {
        UnityEngine.Debug.Log("Save file to: " + args.SavePath);
      }
      else if (videoCapture.captureType == CaptureType.LIVE)
      {
        UnityEngine.Debug.Log("Live stream to: " + args.SavePath);
      }
    }

    private void HandleCaptureError(object sender, CaptureErrorEventArgs args)
    {
      //UnityEngine.Debug.Log(args.ErrorCode);
    }

    private void OnGUI()
    {
      if (
        videoCapture.status == CaptureStatus.READY)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture"))
        {
          videoCapture.StartCapture();
        }
      }
      else if (videoCapture.status == CaptureStatus.STARTED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
        {
          videoCapture.StopCapture();
        }

        if (GUI.Button(new Rect(170, Screen.height - 60, 150, 50), "Cancel Capture"))
        {
          videoCapture.CancelCapture();
        }
      }
      else if (videoCapture.status == CaptureStatus.PENDING)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Muxing"))
        {
          // Waiting processing end
        }
      }
      else if (videoCapture.status == CaptureStatus.STOPPED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Encoding"))
        {
          // Waiting processing end
        }
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(videoCapture.saveFolder);
      }
    }
  }
}