/* Copyright (c) 2019-present Evereal. All rights reserved. */

using System;
using System.IO;
//using System.Runtime.InteropServices;
using UnityEngine;

namespace Evereal.VideoCapture
{
  // This script will record target audio listener sample and encode to audio file.
  public class AudioRecorder : MonoBehaviour, IRecorder
  {
    #region Dll Import

    //[DllImport("FFmpegEncoder")]
    //private static extern IntPtr FFmpegEncoder_StartAudioCapture(int sampleRate, string audioPath, string ffmpegPath);

    //[DllImport("FFmpegEncoder")]
    //private static extern bool FFmpegEncoder_CaptureAudioFrame(IntPtr api, byte[] data);

    //[DllImport("FFmpegEncoder")]
    //private static extern bool FFmpegEncoder_StopAudioCapture(IntPtr api);

    //[DllImport("FFmpegEncoder")]
    //private static extern void FFmpegEncoder_CleanAudioCapture(IntPtr api);

    #endregion

    #region Properties

    public static AudioRecorder singleton;

    // If set live streaming mode, will encoded slice audio files.
    public CaptureType captureType = CaptureType.VOD;

    // Event delegate callback for complete.
    public delegate void OnCompleteEvent(string savePath);
    // Event delegate callback for error.
    public delegate void OnErrorEvent(EncoderErrorCode error);
    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // The captured audio path
    public string audioSavePath;
    // Audio slice for live streaming.
    private string audioSlicePath;

    public string saveFolderFullPath { get; set; }

    // Is audio record started
    public bool recordStarted { get; private set; }
    // Record sample rate
    private int outputSampleRate;

    //private int bufferSize;
    //private int numBuffers;
    private int headerSize = 44; // default for uncompressed wav
    private FileStream fileStream;

    //// Reference to native lib API
    //private IntPtr nativeAPI;
    //// The audio capture prepare vars
    //private IntPtr audioPointer;
    //private Byte[] audioByteBuffer;

    private bool liveSyncCycle;

    // Log message format template
    private string LOG_FORMAT = "[AudioRecorder] {0}";

    #endregion

    #region Audio Recorder

    // If record started
    public bool RecordStarted()
    {
      return recordStarted;
    }

    // Start capture audio session
    public bool StartRecord()
    {
      // Check if we can start capture session
      if (recordStarted)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS);
        return false;
      }

      // Init audio save destination
      if (captureType == CaptureType.VOD)
      {
        audioSavePath = string.Format("{0}audio_{1}_{2}.wav",
          saveFolderFullPath,
          Utils.GetTimeString(),
          Utils.GetRandomString(5));
      }
      else if (captureType == CaptureType.LIVE)
      {
        audioSlicePath = string.Format("{0}{1}.wav",
          saveFolderFullPath,
          Utils.GetTimestampString());
      }

      outputSampleRate = AudioSettings.outputSampleRate;

      if (!StartWrite())
      {
        return false;
      }

      recordStarted = true;

      return true;
    }

    // Stop capture audio session
    public bool StopRecord()
    {
      if (!recordStarted)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture session not start yet!");
        return false;
      }

      recordStarted = false;

      //FFmpegEncoder_StopAudioCapture(nativeAPI);
      // write header
      WriteHeader();

      if (captureType == CaptureType.VOD)
      {
        OnComplete(audioSavePath);
      }
      if (captureType == CaptureType.LIVE)
      {
        OnComplete(audioSlicePath);
      }

      //FFmpegEncoder_CleanAudioCapture(nativeAPI);

      //Debug.LogFormat(LOG_FORMAT, "Audio recorder process finish!");
      return true;
    }

    // Cancel capture audio session
    public bool CancelRecord()
    {
      if (!recordStarted)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture session not start yet!");
        return false;
      }

      recordStarted = false;

      //FFmpegEncoder_StopAudioCapture(nativeAPI);
      //FFmpegEncoder_CleanAudioCapture(nativeAPI);
      fileStream.Close();

      if (File.Exists(audioSavePath))
        File.Delete(audioSavePath);
      audioSavePath = "";
      if (File.Exists(audioSlicePath))
        File.Delete(audioSlicePath);
      audioSlicePath = "";

      //Debug.LogFormat(LOG_FORMAT, "Audio encode process canceled!");
      return true;
    }

    public void SetLiveSyncCycle()
    {
      liveSyncCycle = true;
    }

    public string GetRecordedAudio()
    {
      return audioSavePath;
    }

    private bool StartWrite()
    {
      string audioPath = captureType == CaptureType.LIVE ? audioSlicePath : audioSavePath;

      //nativeAPI = FFmpegEncoder_StartAudioCapture(
      //  outputSampleRate,
      //  audioPath,
      //  FFmpeg.path);
      //if (nativeAPI == IntPtr.Zero)
      //{
      //  OnError(EncoderErrorCode.AUD_FAILED_TO_START);
      //  return false;
      //}
      //// Init temp variables
      //audioByteBuffer = new Byte[8192];
      //GCHandle audioHandle = GCHandle.Alloc(audioByteBuffer, GCHandleType.Pinned);
      //audioPointer = audioHandle.AddrOfPinnedObject();

      fileStream = new FileStream(audioPath, FileMode.Create);

      byte emptyByte = new byte();
      //preparing the header
      for (int i = 0; i < headerSize; i++)
      {
        fileStream.WriteByte(emptyByte);
      }

      return true;
    }

    private void ConvertAndWrite(float[] dataSource)
    {
      //Marshal.Copy(dataSource, 0, audioPointer, 2048);
      //FFmpegEncoder_CaptureAudioFrame(nativeAPI, audioByteBuffer);

      Int16[] intData = new Int16[dataSource.Length];
      // converting in 2 steps : float[] to Int16[], then Int16[] to Byte[]
      Byte[] bytesData = new Byte[dataSource.Length * 2];
      // bytesData array is twice the size of dataSource array because a float converted in Int16 is 2 bytes.
      // to convert float to Int16
      int rescaleFactor = 32767;
      for (int i = 0; i < dataSource.Length; i++)
      {
        intData[i] = (Int16)(dataSource[i] * rescaleFactor);
        Byte[] byteArr = new Byte[2];
        byteArr = BitConverter.GetBytes(intData[i]);
        byteArr.CopyTo(bytesData, i * 2);
      }

      fileStream.Write(bytesData, 0, bytesData.Length);
    }

    private void WriteHeader()
    {
      // FFmpegEncoder_StopAudioCapture(nativeAPI);
      // // Clean audio capture resources
      // FFmpegEncoder_CleanAudioCapture(nativeAPI);

      fileStream.Seek(0, SeekOrigin.Begin);

      Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
      fileStream.Write(riff, 0, 4);

      Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
      fileStream.Write(chunkSize, 0, 4);

      Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
      fileStream.Write(wave, 0, 4);

      Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
      fileStream.Write(fmt, 0, 4);

      Byte[] subChunk1 = BitConverter.GetBytes(16);
      fileStream.Write(subChunk1, 0, 4);

      UInt16 two = 2;
      UInt16 one = 1;

      Byte[] audioFormat = BitConverter.GetBytes(one);
      fileStream.Write(audioFormat, 0, 2);

      Byte[] numChannels = BitConverter.GetBytes(two);
      fileStream.Write(numChannels, 0, 2);

      Byte[] sampleRate = BitConverter.GetBytes(outputSampleRate);
      fileStream.Write(sampleRate, 0, 4);

      Byte[] byteRate = BitConverter.GetBytes(outputSampleRate * 4);
      // sampleRate * bytesPerSample*number of channels, here 44100*2*2

      fileStream.Write(byteRate, 0, 4);

      UInt16 four = 4;
      Byte[] blockAlign = BitConverter.GetBytes(four);
      fileStream.Write(blockAlign, 0, 2);

      UInt16 sixteen = 16;
      Byte[] bitsPerSample = BitConverter.GetBytes(sixteen);
      fileStream.Write(bitsPerSample, 0, 2);

      Byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
      fileStream.Write(dataString, 0, 4);

      Byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length - headerSize);
      fileStream.Write(subChunk2, 0, 4);

      fileStream.Close();
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
      if (singleton != null)
        return;
      singleton = this;

      recordStarted = false;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
      if (recordStarted)
      {
        // audio data is interlaced
        ConvertAndWrite(data);

        // slice audio into different files for live stream
        if (captureType == CaptureType.LIVE && liveSyncCycle)
        {
          liveSyncCycle = false;

          WriteHeader();

          FFmpegStreamer.singleton.EnqueueAudioSlice(audioSlicePath);

          audioSlicePath = string.Format("{0}{1}.wav",
            saveFolderFullPath,
            Utils.GetTimestampString());

          // restart
          StartWrite();
        }
      }
    }

    private void OnDestroy()
    {
      if (recordStarted)
      {
        StopRecord();
      }
    }

    #endregion
  }
}