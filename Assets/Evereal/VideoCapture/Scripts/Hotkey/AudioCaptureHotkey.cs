/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(AudioCapture))]
  public class AudioCaptureHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public KeyCode stopCapture = KeyCode.F2;
    public KeyCode cancelCapture = KeyCode.F3;
    public bool showHintUI = true;

    private AudioCapture audioCapture;

    private void Awake()
    {
      audioCapture = GetComponent<AudioCapture>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyUp(startCapture))
      {
        audioCapture.StartCapture();
      }
      else if (Input.GetKeyUp(stopCapture))
      {
        audioCapture.StopCapture();
      }
      else if (Input.GetKeyUp(cancelCapture))
      {
        audioCapture.CancelCapture();
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