/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(VideoCaptureManager))]
  public class VideoCaptureManagerHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public KeyCode stopCapture = KeyCode.F2;
    public KeyCode cancelCapture = KeyCode.F3;
    public bool showHintUI = true;

    private VideoCaptureManager videoCaptureManager;

    private void Awake()
    {
      videoCaptureManager = GetComponent<VideoCaptureManager>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyUp(startCapture))
      {
        bool pending = false;
        // check if still processing
        foreach (VideoCapture videoCapture in videoCaptureManager.videoCaptures)
        {
          if (videoCapture.status == CaptureStatus.STOPPED ||
          videoCapture.status == CaptureStatus.PENDING)
          {
            pending = true;
            break;
          }
        }
        if (pending)
          return;
        videoCaptureManager.StartCapture();
      }
      else if (Input.GetKeyUp(stopCapture))
      {
        videoCaptureManager.StopCapture();
      }
      else if (Input.GetKeyUp(cancelCapture))
      {
        videoCaptureManager.CancelCapture();
      }
    }

    void OnGUI()
    {
      if (showHintUI)
      {
        GUI.Label(new Rect(10, 10, 200, 20), startCapture.ToString() + ": Start Capture");
        GUI.Label(new Rect(10, 30, 200, 20), stopCapture.ToString() + ": Stop Capture");
        GUI.Label(new Rect(10, 50, 200, 20), cancelCapture.ToString() + ": Cancel Capture");
      }
    }
  }
}