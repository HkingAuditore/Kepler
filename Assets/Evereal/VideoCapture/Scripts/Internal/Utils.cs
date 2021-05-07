/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Evereal.VideoCapture
{
  public class Utils
  {
    private static System.Random random = new System.Random();

    /// <summary>
    /// Save render texture to PNG image file.
    /// </summary>
    /// <param name="rtex">RenderTexture.</param>
    /// <param name="fileName">File name.</param>
    public static void RenderTextureToPNG(RenderTexture rtex, string fileName)
    {
      Texture2D tex = new Texture2D(rtex.width, rtex.height, TextureFormat.RGB24, false);
      RenderTexture.active = rtex;
      tex.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0, false);
      RenderTexture.active = null;
      TextureToPNG(tex, fileName);
    }

    /// <summary>
    /// Save texture to PNG image file.
    /// </summary>
    /// <param name="tex">Tex.</param>
    /// <param name="fileName">File name.</param>
    public static void TextureToPNG(Texture2D tex, string fileName)
    {
      string filePath = "Captures/" + fileName;
      byte[] imageBytes = tex.EncodeToPNG();
      System.IO.File.WriteAllBytes(filePath, imageBytes);
      Debug.Log("Save texture " + filePath);
    }

    /// <summary>
    /// Save render texture to JPG image file.
    /// </summary>
    /// <param name="rtex">RenderTexture.</param>
    /// <param name="fileName">File name.</param>
    public static void RenderTextureToJPG(RenderTexture rtex, string fileName)
    {
      Texture2D tex = new Texture2D(rtex.width, rtex.height, TextureFormat.RGB24, false);
      RenderTexture.active = rtex;
      tex.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0, false);
      RenderTexture.active = null;
      TextureToJPG(tex, fileName);
    }

    /// <summary>
    /// Save texture to JPG image file.
    /// </summary>
    /// <param name="tex">Tex.</param>
    /// <param name="fileName">File name.</param>
    public static void TextureToJPG(Texture2D tex, string fileName)
    {
      string filePath = "Captures/" + fileName;
      byte[] imageBytes = tex.EncodeToJPG();
      System.IO.File.WriteAllBytes(filePath, imageBytes);
      Debug.Log("Save texture " + filePath);
    }

    /// <summary>
    /// Create materials which will be used for equirect and cubemap generation.
    /// </summary>
    /// <param name="sName"> shader name </param>
    /// <param name="m2Create"> material </param>
    /// <returns></returns>
    public static Material CreateMaterial(string sName, Material m2Create)
    {
      if (m2Create && (m2Create.shader.name == sName))
        return m2Create;
      Shader s = Shader.Find(sName);
      return CreateMaterial(s, m2Create);
    }

    /// <summary>
    /// Create materials which will be used for equirect and cubemap generation.
    /// </summary>
    /// <param name="s"> shader code </param>
    /// <param name="m2Create"> material </param>
    /// <returns></returns>
    public static Material CreateMaterial(Shader s, Material m2Create)
    {
      if (!s)
      {
        Debug.Log("Create material missing shader!");
        return null;
      }

      if (m2Create && (m2Create.shader == s) && (s.isSupported))
        return m2Create;

      if (!s.isSupported)
      {
        return null;
      }

      if (m2Create != null)
      {
        UnityEngine.Object.Destroy(m2Create);
      }

      m2Create = new Material(s);
      m2Create.hideFlags = HideFlags.DontSave;

      return m2Create;
    }

    /// <summary>
    /// Create RenderTexture.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="antiAliasing"></param>
    /// <param name="t2Create"></param>
    /// <param name="create"></param>
    /// <returns></returns>
    public static RenderTexture CreateRenderTexture(int width, int height, int depth, int antiAliasing, RenderTexture t2Create, bool create = true)
    {
      if (t2Create &&
        (t2Create.width == width) && (t2Create.height == height) && (t2Create.depth == depth) &&
        (t2Create.antiAliasing == antiAliasing) && (t2Create.IsCreated() == create))
        return t2Create;

      if (t2Create != null)
      {
        UnityEngine.Object.Destroy(t2Create);
      }

      t2Create = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32);
      //t2Create = new RenderTexture(width, height, depth, RenderTextureFormat.Default);
      t2Create.antiAliasing = antiAliasing;
      t2Create.hideFlags = HideFlags.HideAndDontSave;

      // Make sure render texture is created.
      if (create)
        t2Create.Create();

      return t2Create;
    }

    /// <summary>
    /// Create Cubemap.
    /// </summary>
    /// <param name="cubemapSize"></param>
    /// <param name="c2Create"></param>
    /// <returns></returns>
    public static Cubemap CreateCubemap(int cubemapSize, Cubemap c2Create)
    {
      if (c2Create && c2Create.width == cubemapSize)
        return c2Create;

      if (c2Create != null)
      {
        UnityEngine.Object.Destroy(c2Create);
      }

      c2Create = new Cubemap(cubemapSize, TextureFormat.RGB24, false);

      return c2Create;
    }

    /// <summary>
    /// Create Texture2D.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="t2Create"></param>
    /// <returns></returns>
    public static Texture2D CreateTexture(int width, int height, Texture2D t2Create)
    {
      if (t2Create && (t2Create.width == width) && (t2Create.height == height))
        return t2Create;

      if (t2Create != null)
      {
        UnityEngine.Object.Destroy(t2Create);
      }

      t2Create = new Texture2D(width, height, TextureFormat.RGBA32, false);
      t2Create.hideFlags = HideFlags.HideAndDontSave;

      return t2Create;
    }

    /// <summary>
    /// Generate random string.
    /// </summary>
    /// <param name="length">random string length</param>
    /// <returns></returns>
    public static string GetRandomString(int length)
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GetTimeString()
    {
      return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    public static string GetTimestampString()
    {
      return DateTime.Now.ToString("yyyyMMddHHmmssff");
    }

    public static int GetClosestEvenNumber(int n)
    {
      if (n % 2 == 1)
        return n - 1;
      return n;
    }

    public static bool isWindows64Bit()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      string sysInfo = SystemInfo.operatingSystem;
      return sysInfo.Substring(sysInfo.Length - 5).Equals("64bit");
#else
      return false;
#endif
    }

    public static void EncodeVideo4K(string videoFile)
    {
      if (string.IsNullOrEmpty(videoFile))
      {
        return;
      }
      if (!System.IO.File.Exists(videoFile))
      {
        Debug.LogWarning("Video file " + videoFile + " is not found");
        return;
      }
      Debug.Log("Start encode video " + videoFile + " to 4K");
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(FFmpegConfig.path, " -i \"" + videoFile + "\" -s 4096x2160  \"" + videoFile.Replace(ext, "_4K" + ext) + "\"");
    }

    public static void EncodeVideo8K(string videoFile)
    {
      if (string.IsNullOrEmpty(videoFile))
      {
        return;
      }
      if (!System.IO.File.Exists(videoFile))
      {
        Debug.LogWarning("Video file " + videoFile + " is not found");
        return;
      }
      Debug.Log("Start encode video " + videoFile + " to 8K");
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(FFmpegConfig.path, " -i \"" + videoFile + "\" -s 7680x4320  \"" + videoFile.Replace(ext, "_8K" + ext) + "\"");
    }

    public static void ConvertVideoWmv(string videoFile)
    {
      if (string.IsNullOrEmpty(videoFile))
      {
        return;
      }
      if (!System.IO.File.Exists(videoFile))
      {
        Debug.LogWarning("Video file " + videoFile + " is not found");
        return;
      }
      Debug.Log("Start convert video " + videoFile + " to WMV");
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(FFmpegConfig.path, " -i \"" + videoFile + "\" -qscale 2 -vcodec msmpeg4 -acodec wmav2 \"" + videoFile.Replace(ext, ".wmv") + "\"");
    }

    public static void ConvertVideoAvi(string videoFile)
    {
      if (string.IsNullOrEmpty(videoFile))
      {
        return;
      }
      if (!System.IO.File.Exists(videoFile))
      {
        Debug.LogWarning("Video file " + videoFile + " is not found");
        return;
      }
      Debug.Log("Start convert video " + videoFile + " to AVI");
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(FFmpegConfig.path, " -i \"" + videoFile + "\" -vcodec copy -acodec copy \"" + videoFile.Replace(ext, ".avi") + "\"");
    }

    public static void ConvertVideoFlv(string videoFile)
    {
      if (string.IsNullOrEmpty(videoFile))
      {
        return;
      }
      if (!System.IO.File.Exists(videoFile))
      {
        Debug.LogWarning("Video file " + videoFile + " is not found");
        return;
      }
      Debug.Log("Start convert video " + videoFile + " to FLV");
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(FFmpegConfig.path, " -i \"" + videoFile + "\" -c:v libx264 -crf 19 \"" + videoFile.Replace(ext, ".flv") + "\"");
    }

    public static void ConvertVideoMkv(string videoFile)
    {
      if (string.IsNullOrEmpty(videoFile))
      {
        return;
      }
      if (!System.IO.File.Exists(videoFile))
      {
        Debug.LogWarning("Video file " + videoFile + " is not found");
        return;
      }
      Debug.Log("Start convert video " + videoFile + " to MKV");
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(FFmpegConfig.path, " -i \"" + videoFile + "\" -vcodec copy -acodec copy \"" + videoFile.Replace(ext, ".mkv") + "\"");
    }

    public static void ConvertVideoGif(string videoFile)
    {
      if (string.IsNullOrEmpty(videoFile))
      {
        return;
      }
      if (!System.IO.File.Exists(videoFile))
      {
        Debug.LogWarning("Video file " + videoFile + " is not found");
        return;
      }
      Debug.Log("Start convert video " + videoFile + " to GIF");
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(FFmpegConfig.path, " -i \"" + videoFile + "\" -s 1280x720 -pix_fmt rgb24 \"" + videoFile.Replace(ext, ".gif") + "\"");
    }

    public static string GetEncoderPresetExt(EncoderPreset preset)
    {
      switch (preset)
      {
        case EncoderPreset.H264_MP4:
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        case EncoderPreset.H264_NVIDIA_MP4:
#endif
        case EncoderPreset.H264_LOSSLESS_MP4:
        case EncoderPreset.H265_MP4:
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        case EncoderPreset.H265_NVIDIA_MP4:
#endif
          return "mp4";
        case EncoderPreset.VP8_WEBM:
        case EncoderPreset.VP9_WEBM:
          return "webm";
        case EncoderPreset.FFV1_MKV:
          return "mkv";
      }
      return null;
    }

    public static string CreateFolder(string f2Create)
    {
      string folder = f2Create;
      if (string.IsNullOrEmpty(folder))
      {
        folder = "Captures/";
      }
      if (!folder.EndsWith("/") && !folder.EndsWith("\\"))
      {
        folder += "/";
      }
      if (!Directory.Exists(folder))
      {
        Directory.CreateDirectory(folder);
      }
      return Path.GetFullPath(folder);
    }

    public static void BrowseFolder(string folder)
    {
      if (string.IsNullOrEmpty(folder))
      {
        folder = "Captures/";
      }
      string fullPath = Path.GetFullPath(folder);
      if (Directory.Exists(fullPath))
      {
        Process.Start(Path.GetFullPath(folder));
      }
      else
      {
        Debug.LogWarning("Folder " + fullPath + " not existed!");
      }
    }

    public static void WriteLogToDisk(string msg)
    {
      StreamWriter writer = new StreamWriter("UnityLog.txt", true);
      writer.WriteLine(msg);
      writer.Close();
    }
  }
}