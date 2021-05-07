/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(ImageCapture))]
  public class ImageCaptureGUI : MonoBehaviour
  {
    private ImageCapture imageCapture;

    private void Awake()
    {
      imageCapture = GetComponent<ImageCapture>();
      Application.runInBackground = true;
    }

    private void OnEnable()
    {
      imageCapture.OnComplete += HandleCaptureComplete;
      imageCapture.OnError += HandleCaptureError;
    }

    private void OnDisable()
    {
      imageCapture.OnComplete -= HandleCaptureComplete;
      imageCapture.OnError -= HandleCaptureError;
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
        imageCapture.status == CaptureStatus.READY)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture"))
        {
          imageCapture.StartCapture();
        }
      }
      else if (imageCapture.status == CaptureStatus.STARTED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Encoding"))
        {
          // Waiting processing end
        }
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Browse"))
      {
        // Open video save directory
        Utils.BrowseFolder(imageCapture.saveFolder);
      }
    }
  }
}