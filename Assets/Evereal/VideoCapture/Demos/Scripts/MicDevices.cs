/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  public class MicDevices : MonoBehaviour
  {
    // Use this for initialization
    void Start()
    {
#if !UNITY_WEBGL
      Debug.Log("Microphone Devices Info:");
      for (int i = 0; i < Microphone.devices.Length; i++)
      {
        Debug.LogFormat("Device Index {0}: {1}", i, Microphone.devices[i]);
      }
#endif
    }
  }
}