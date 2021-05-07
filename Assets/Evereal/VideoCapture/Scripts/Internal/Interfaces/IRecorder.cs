/* Copyright (c) 2019-present Evereal. All rights reserved. */

namespace Evereal.VideoCapture
{
  public interface IRecorder
  {
    // If record started
    bool RecordStarted();

    // Start record
    bool StartRecord();

    // Stop record
    bool StopRecord();

    // Cancel record
    bool CancelRecord();

    // Get video save path
    string GetRecordedAudio();
  }
}