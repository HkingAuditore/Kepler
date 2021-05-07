/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  public class DisplayTime : MonoBehaviour
  {
    private void OnGUI()
    {
      GUI.Label(new Rect(Screen.width / 2 - 70, Screen.height - 40, 140, 20), System.DateTime.Now.ToString());
    }
  }
}