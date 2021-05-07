/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture {

	[RequireComponent(typeof(VideoCaptureManager))]
	public class VideoCaptureManagerGUI : MonoBehaviour {

		private VideoCaptureManager videoCaptureManager;

		private void Awake()
    {
      videoCaptureManager = GetComponent<VideoCaptureManager>();
      Application.runInBackground = true;
    }

    private void OnEnable()
    {
      foreach (VideoCapture videoCapture in videoCaptureManager.videoCaptures)
      {
        videoCapture.OnComplete += HandleCaptureComplete;
        videoCapture.OnError += HandleCaptureError;
      }
    }

    private void OnDisable()
    {
      foreach (VideoCapture videoCapture in videoCaptureManager.videoCaptures)
      {
        videoCapture.OnComplete -= HandleCaptureComplete;
        videoCapture.OnError -= HandleCaptureError;
      }
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
			if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(videoCaptureManager.saveFolder);
      }
			bool stopped = false;
			bool pending = false;
			// check if still processing
			foreach (VideoCapture videoCapture in videoCaptureManager.videoCaptures) {
				if (videoCapture.status == CaptureStatus.STOPPED) {
					stopped = true;
					break;
				}
				if (videoCapture.status == CaptureStatus.PENDING) {
					pending = true;
					break;
				}
			}
			if (stopped) {
				if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Encoding")) {
          // Waiting processing end
        }
				return;
			}
			if (pending) {
				if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Muxing")) {
          // Waiting processing end
        }
				return;
			}
			if (videoCaptureManager.captureStarted) {
  			if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture")) {
          videoCaptureManager.StopCapture();
        }
        if (GUI.Button(new Rect(170, Screen.height - 60, 150, 50), "Cancel Capture"))
        {
          videoCaptureManager.CancelCapture();
        }
      }
			else {
				if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture")) {
          videoCaptureManager.StartCapture();
        }
			}
    }
	}
}