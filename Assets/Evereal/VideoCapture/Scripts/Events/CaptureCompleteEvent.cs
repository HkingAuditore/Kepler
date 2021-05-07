/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Define a class to hold capture complete event info
  /// </summary>
  public class CaptureCompleteEventArgs : EventArgs
  {
    public CaptureCompleteEventArgs(string s)
    {
      savePath = s;
    }

    private string savePath;

    public string SavePath
    {
      get
      {
        return savePath;
      }
      protected set
      {
        savePath = value;
      }
    }
  }
}