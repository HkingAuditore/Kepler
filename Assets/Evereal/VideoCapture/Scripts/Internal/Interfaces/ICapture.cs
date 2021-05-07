/* Copyright (c) 2019-present Evereal. All rights reserved. */

namespace Evereal.VideoCapture
{
  public interface ICapture
  {
    // Get or set the current status
    CaptureStatus status { get; }

    // Start capture
    bool StartCapture();

    // Stop capture
    bool StopCapture();

    // Cancel capture
    bool CancelCapture();
  }
}