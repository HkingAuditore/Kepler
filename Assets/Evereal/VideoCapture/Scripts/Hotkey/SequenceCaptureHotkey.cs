/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(SequenceCapture))]
  public class SequenceCaptureHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public KeyCode stopCapture = KeyCode.F2;
    public KeyCode cancelCapture = KeyCode.F3;
    public bool showHintUI = true;

    private SequenceCapture sequenceCapture;

    private void Awake()
    {
      sequenceCapture = GetComponent<SequenceCapture>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyUp(startCapture))
      {
        sequenceCapture.StartCapture();
      }
      else if (Input.GetKeyUp(stopCapture))
      {
        sequenceCapture.StopCapture();
      }
      else if (Input.GetKeyUp(cancelCapture))
      {
        sequenceCapture.CancelCapture();
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