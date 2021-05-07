/* Copyright (c) 2019-present Evereal. All rights reserved. */

namespace Evereal.VideoCapture
{
  public interface IVideoCapture : ICapture
  {
    // When video encoding complete
    void OnEncoderComplete(string path);

    // When audio muxing complete
    void OnMuxerComplete(string path);

    // When transcode complete
    void OnTranscodeComplete(string path);

    // Get encoder instance
    EncoderBase GetEncoder();
  }
}