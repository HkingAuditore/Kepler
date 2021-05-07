/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(ImageCapture))]
  public class ImageCaptureHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public bool showHintUI = true;

    private ImageCapture imageCapture;

    private void Awake()
    {
      imageCapture = GetComponent<ImageCapture>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyUp(startCapture))
      {
        imageCapture.StartCapture();
      }
    }

    void OnGUI()
    {
      if (showHintUI)
      {
        GUI.Label(new Rect(10, 10, 200, 20), startCapture.ToString() + ": Start Capture");
      }
    }
  }
}