/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(ScreenShot))]
  public class ScreenShotGUI : MonoBehaviour
  {
    private ScreenShot screenShot;

    private void Awake()
    {
      screenShot = GetComponent<ScreenShot>();
      Application.runInBackground = true;
    }

    private void OnEnable()
    {
      screenShot.OnComplete += HandleCaptureComplete;
      screenShot.OnError += HandleCaptureError;
    }

    private void OnDisable()
    {
      screenShot.OnComplete -= HandleCaptureComplete;
      screenShot.OnError -= HandleCaptureError;
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
      if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Take Screenshot"))
      {
        screenShot.StartCapture();
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Browse"))
      {
        // Open image save directory
        Utils.BrowseFolder(screenShot.saveFolder);
      }
    }
  }
}