/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Define a class to hold capture error event info
  /// </summary>
  public class CaptureErrorEventArgs : EventArgs
  {
    public CaptureErrorEventArgs(CaptureErrorCode e)
    {
      errorCode = e;
    }

    private CaptureErrorCode errorCode;

    public CaptureErrorCode ErrorCode
    {
      get
      {
        return errorCode;
      }
      protected set
      {
        errorCode = value;
      }
    }
  }
}