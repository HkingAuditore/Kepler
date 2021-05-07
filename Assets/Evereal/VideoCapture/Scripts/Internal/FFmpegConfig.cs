/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System.IO;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// FFmpeg executable utility.
  /// </summary>
  public class FFmpegConfig
  {
    // The ffmpeg folder
    public static string folder
    {
      get
      {
        return Application.streamingAssetsPath + "/FFmpeg/";
      }
    }

    // The 32bit ffmpeg path for Windows
    public static string windows32Path
    {
      get
      {
        return folder + "x86/ffmpeg.exe";
      }
    }

    // The 32bit ffmpeg path for Windows
    public static string windows64Path
    {
      get
      {
        return folder + "x86_64/ffmpeg.exe";
      }
    }

    // The ffmpeg path for macOS
    public static string macOSPath
    {
      get
      {
        return folder + "ffmpeg";
      }
    }

    // Get ffmpeg path
    public static string path
    {
      get
      {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (Utils.isWindows64Bit())
        {
          return windows64Path;
        }
        return windows32Path;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        return macOSPath;
#else
        return "";
#endif
      }
    }

    public static bool IsExist()
    {
      return File.Exists(path);
    }

    public static void CheckFolder()
    {
      if (!Directory.Exists(folder))
      {
        Directory.CreateDirectory(folder);
      }
      string win64Folder = folder + "x86_64/";
      if (!Directory.Exists(win64Folder))
      {
        Directory.CreateDirectory(win64Folder);
      }
      string win32Folder = folder + "x86/";
      if (!Directory.Exists(win32Folder))
      {
        Directory.CreateDirectory(win32Folder);
      }
    }
  }
}