/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System.Threading;
using UnityEngine;
using UnityEditor;

namespace Evereal.VideoCapture
{
  public class MenuEditor : MonoBehaviour
  {
    private const string WINDOWS_FFMPEG_32_DOWNLOAD_URL = "https://evereal.s3-us-west-1.amazonaws.com/ffmpeg/4.2/x86/ffmpeg.exe";
    private const string WINDOWS_FFMPEG_64_DOWNLOAD_URL = "https://evereal.s3-us-west-1.amazonaws.com/ffmpeg/4.2/x86_64/ffmpeg.exe";
    private const string OSX_FFMPEG_DOWNLOAD_URL = "https://evereal.s3-us-west-1.amazonaws.com/ffmpeg/4.2.2/ffmpeg";

    private static Thread downloadFFmpegThread;

    [MenuItem("Tools/Evereal/VideoCapture/GameObject/VideoCapture")]
    private static void CreateVideoCaptureObject(MenuCommand menuCommand)
    {
      GameObject videoCapturePrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/VideoCapture")) as GameObject;
      videoCapturePrefab.name = "VideoCapture";
      //PrefabUtility.DisconnectPrefabInstance(videoCapturePrefab);
      GameObjectUtility.SetParentAndAlign(videoCapturePrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(videoCapturePrefab, "Create " + videoCapturePrefab.name);
      Selection.activeObject = videoCapturePrefab;
    }

    [MenuItem("Tools/Evereal/VideoCapture/GameObject/SequenceCapture")]
    private static void CreateSequenceCaptureObject(MenuCommand menuCommand)
    {
      GameObject sequenceCapturePrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/SequenceCapture")) as GameObject;
      sequenceCapturePrefab.name = "SequenceCapture";
      //PrefabUtility.DisconnectPrefabInstance(sequenceCapturePrefab);
      GameObjectUtility.SetParentAndAlign(sequenceCapturePrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(sequenceCapturePrefab, "Create " + sequenceCapturePrefab.name);
      Selection.activeObject = sequenceCapturePrefab;
    }

    [MenuItem("Tools/Evereal/VideoCapture/GameObject/ImageCapture")]
    private static void CreateImageCaptureObject(MenuCommand menuCommand)
    {
      GameObject imageCapturePrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/ImageCapture")) as GameObject;
      imageCapturePrefab.name = "ImageCapture";
      //PrefabUtility.DisconnectPrefabInstance(sequenceCapturePrefab);
      GameObjectUtility.SetParentAndAlign(imageCapturePrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(imageCapturePrefab, "Create " + imageCapturePrefab.name);
      Selection.activeObject = imageCapturePrefab;
    }

    [MenuItem("Tools/Evereal/VideoCapture/GameObject/AudioCapture")]
    private static void CreateAudioCaptureObject(MenuCommand menuCommand)
    {
      GameObject audioCapturePrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/AudioCapture")) as GameObject;
      audioCapturePrefab.name = "AudioCapture";
      //PrefabUtility.DisconnectPrefabInstance(audioCapturePrefab);
      GameObjectUtility.SetParentAndAlign(audioCapturePrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(audioCapturePrefab, "Create " + audioCapturePrefab.name);
      Selection.activeObject = audioCapturePrefab;
    }

    // [MenuItem("Tools/Evereal/VideoCapture/GameObject/ScreenShot")]
    // private static void CreateScreenShotObject(MenuCommand menuCommand)
    // {
    //   GameObject screenshotPrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/Legacy/ScreenShot")) as GameObject;
    //   screenshotPrefab.name = "ScreenShot";
    //   //PrefabUtility.DisconnectPrefabInstance(screenshotPrefab);
    //   GameObjectUtility.SetParentAndAlign(screenshotPrefab, menuCommand.context as GameObject);
    //   Undo.RegisterCreatedObjectUndo(screenshotPrefab, "Create " + screenshotPrefab.name);
    //   Selection.activeObject = screenshotPrefab;
    // }

    [MenuItem("Tools/Evereal/VideoCapture/FFmpeg/Download Windows Build (32 bit)")]
    private static void DownloadFFmpeg32ForWindows()
    {
      FFmpegConfig.CheckFolder();

      if (downloadFFmpegThread != null)
      {
        if (downloadFFmpegThread.IsAlive)
          downloadFFmpegThread.Abort();
        downloadFFmpegThread = null;
      }
      string windowsFFmpegPath = FFmpegConfig.windows32Path;
      downloadFFmpegThread = new Thread(
        () => DownloadFFmpegThreadFunction(WINDOWS_FFMPEG_32_DOWNLOAD_URL, windowsFFmpegPath));
      downloadFFmpegThread.Priority = System.Threading.ThreadPriority.Lowest;
      downloadFFmpegThread.IsBackground = true;
      downloadFFmpegThread.Start();
    }

    [MenuItem("Tools/Evereal/VideoCapture/FFmpeg/Download Windows Build (64 bit)")]
    private static void DownloadFFmpeg64ForWindows()
    {
      FFmpegConfig.CheckFolder();

      if (downloadFFmpegThread != null)
      {
        if (downloadFFmpegThread.IsAlive)
          downloadFFmpegThread.Abort();
        downloadFFmpegThread = null;
      }
      string windowsFFmpegPath = FFmpegConfig.windows64Path;
      downloadFFmpegThread = new Thread(
        () => DownloadFFmpegThreadFunction(WINDOWS_FFMPEG_64_DOWNLOAD_URL, windowsFFmpegPath));
      downloadFFmpegThread.Priority = System.Threading.ThreadPriority.Lowest;
      downloadFFmpegThread.IsBackground = true;
      downloadFFmpegThread.Start();
    }

    [MenuItem("Tools/Evereal/VideoCapture/FFmpeg/Download macOS Build (64 bit)")]
    private static void DownloadFFmpegForOSX()
    {
      FFmpegConfig.CheckFolder();

      if (downloadFFmpegThread != null)
      {
        if (downloadFFmpegThread.IsAlive)
          downloadFFmpegThread.Abort();
        downloadFFmpegThread = null;
      }
      string macOSFFmpegPath = FFmpegConfig.macOSPath;
      downloadFFmpegThread = new Thread(
        () => DownloadFFmpegThreadFunction(OSX_FFMPEG_DOWNLOAD_URL, macOSFFmpegPath));
      downloadFFmpegThread.Priority = System.Threading.ThreadPriority.Lowest;
      downloadFFmpegThread.IsBackground = true;
      downloadFFmpegThread.Start();
    }

    [MenuItem("Tools/Evereal/VideoCapture/FFmpeg/Grant macOS Build Permission")]
    private static void GrantFFmpegPermissionForOSX()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
      Command.Run("chmod", "a+x " + "\"" + FFmpegConfig.macOSPath + "\"");
      UnityEngine.Debug.Log("Grant permission for: " + FFmpegConfig.macOSPath);
#endif
    }

    private static void DownloadFFmpegThreadFunction(string downloadUrl, string savePath)
    {
      UnityEngine.Debug.Log("Download FFmpeg in the background, please wait a few minutes until complete...");
      Command.Run("curl", downloadUrl + " --output " + "\"" + savePath + "\"");

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
      // Grant FFmpeg permission for OSX
      Command.Run("chmod", "a+x " + "\"" + savePath + "\"");
      UnityEngine.Debug.Log("Grant permission for: " + savePath);
#endif

      UnityEngine.Debug.Log("Download FFmpeg complete!");
    }
  }
}