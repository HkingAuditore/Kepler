/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(SequenceCapture))]
  public class SequenceCaptureGUI : MonoBehaviour
  {
    private SequenceCapture sequenceCapture;

    private void Awake()
    {
      sequenceCapture = GetComponent<SequenceCapture>();
      Application.runInBackground = true;
    }

    private void OnEnable()
    {
      sequenceCapture.OnComplete += HandleCaptureComplete;
      sequenceCapture.OnError += HandleCaptureError;
    }

    private void OnDisable()
    {
      sequenceCapture.OnComplete -= HandleCaptureComplete;
      sequenceCapture.OnError -= HandleCaptureError;
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
      if (
        sequenceCapture.status == CaptureStatus.READY)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture"))
        {
          sequenceCapture.StartCapture();
        }
      }
      else if (sequenceCapture.status == CaptureStatus.STARTED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
        {
          sequenceCapture.StopCapture();
        }

        if (GUI.Button(new Rect(170, Screen.height - 60, 150, 50), "Cancel Capture"))
        {
          sequenceCapture.CancelCapture();
        }
      }
      else if (sequenceCapture.status == CaptureStatus.PENDING)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Muxing"))
        {
          // Waiting processing end
        }
      }
      else if (sequenceCapture.status == CaptureStatus.STOPPED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Encoding"))
        {
          // Waiting processing end
        }
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(sequenceCapture.saveFolder);
      }
    }
  }
}