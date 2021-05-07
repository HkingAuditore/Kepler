/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Video cache utils.
  /// </summary>
  public class VideoCache
  {
    // The last recorded video file
    private static string _lastVideoFile = "";
    public static string lastVideoFile
    {
      get
      {
        if (!string.IsNullOrEmpty(_lastVideoFile))
        {
          return _lastVideoFile;
        }
        else if (!string.IsNullOrEmpty(PlayerPrefs.GetString(Constants.LAST_VIDEO_FILE_KEY)))
        {
          return PlayerPrefs.GetString(Constants.LAST_VIDEO_FILE_KEY);
        }

        return "";
      }
      set
      {
        _lastVideoFile = value;
        PlayerPrefs.SetString(Constants.LAST_VIDEO_FILE_KEY, _lastVideoFile);
      }
    }

    public static void CacheLastVideoFile(string videoFile)
    {
      PlayerPrefs.SetString(Constants.LAST_VIDEO_FILE_KEY, videoFile);
    }
  }
}