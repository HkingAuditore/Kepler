using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace TTS
{
    public class VoiceGenerator : MonoBehaviour
    {
        private const string session_begin_params =
            "voice_name = xiaoyan, text_encoding = utf8, sample_rate = 16000, speed = 50, volume = 50, pitch = 50, rdn = 0";

        private byte[] bytes;
        private int    err_code;
        private string offline_session_begin_params;
        private IntPtr session_id;

        private Thread voiceThread;

        public string content { get; set; }

        private void Awake()
        {
            var xiaoyan_path = (Application.dataPath + "PlugIns/TTS/xiaoyan.jet").Replace("/", "\\");
            var common_path = (Application.dataPath + "PlugIns/TTS/common.jet").Replace("/", "\\");
            offline_session_begin_params =
                "engine_type = local, voice_name = xiaoyan, text_encoding = utf8, tts_res_path = fo|" + xiaoyan_path +
                ";fo|" + common_path + ", sample_rate = 16000, speed = 50, volume = 50, pitch = 50, rdn = 0";
        }

        private void Start()
        {
            var message = msc.MSCDLL.MSPLogin("", "", "appid=55b70993,word_dir= . ");
            if (message != (int) msc.Errors.MSP_SUCCESS) Debug.LogError("登录失败！错误信息：" + message);
            Debug.Log("登录成功");
        }

        private void OnDestroy()
        {
            if (voiceThread != null) voiceThread.Abort();
            msc.MSCDLL.MSPLogout();
            Debug.Log("注销成功");
        }


        public void SpeakContent()
        {
            voiceThread = new Thread(Speak);
            voiceThread.Start();
        }

        private void Speak()
        {
            Debug.Log(content);
            Online_TTS(content);
        }

        /// <summary>
        ///     文字转语音
        /// </summary>
        /// <param name="content"></param>
        public void Speak(string content)
        {
            Debug.Log(this.content);
            Online_TTS(content);
        }

        // private void Update()
        // {
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         Online_TTS(speekText);
        //         // Offline_TTS(speekText);
        //     }
        // }

        private void Online_TTS(string speekText)
        {
            //语音合成开始
            session_id = msc.MSCDLL.QTTSSessionBegin(session_begin_params, ref err_code);

            if (err_code != (int) msc.Errors.MSP_SUCCESS)
            {
                Debug.LogError("初始化语音合成失败，错误信息：" + err_code);
                return;
            }

            //语音合成设置文本
            err_code = msc.MSCDLL.QTTSTextPut(session_id, speekText, (uint) Encoding.Default.GetByteCount(speekText),
                                              string.Empty);
            if (err_code != (int) msc.Errors.MSP_SUCCESS)
            {
                Debug.LogError("向服务器发送数据失败，错误信息：" + err_code);
                return;
            }

            uint audio_len    = 0;
            var  synth_status = msc.SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
            var  memoryStream = new MemoryStream();
            memoryStream.Write(new byte[44], 0, 44);
            while (true)
            {
                var source = msc.MSCDLL.QTTSAudioGet(session_id, ref audio_len, ref synth_status, ref err_code);
                var array  = new byte[audio_len];
                if (audio_len > 0) Marshal.Copy(source, array, 0, (int) audio_len);
                memoryStream.Write(array, 0, array.Length);
                Thread.Sleep(150);
                if (synth_status == msc.SynthStatus.MSP_TTS_FLAG_DATA_END || err_code != (int) msc.Errors.MSP_SUCCESS)
                    break;
            }

            err_code = msc.MSCDLL.QTTSSessionEnd(session_id, "");
            if (err_code != (int) msc.Errors.MSP_SUCCESS)
            {
                Debug.LogError("会话结束失败！错误信息: " + err_code);
                return;
            }


            var header     = getWave_Header((int) memoryStream.Length - 44); //创建wav文件头
            var headerByte = StructToBytes(header);                          //把文件头结构转化为字节数组
            memoryStream.Position = 0;                                       //定位到文件头
            memoryStream.Write(headerByte, 0, headerByte.Length);            //写入文件头
            bytes = memoryStream.ToArray();
            memoryStream.Close();
            if (Application.streamingAssetsPath + "/" + name + ".wav" != null)
            {
                if (File.Exists(Application.streamingAssetsPath + "/" + name + ".wav"))
                    File.Delete(Application.streamingAssetsPath + "/" + name + ".wav");
                File.WriteAllBytes(Application.streamingAssetsPath + "/" + name + ".wav", bytes);
                StartCoroutine(OnAudioLoadAndPaly(Application.streamingAssetsPath + "/" + name + ".wav", AudioType.WAV,
                                                  gameObject.GetComponent<AudioSource>()));
            }

            Debug.Log("合成结束成功");
        }

        private void Offline_TTS(string speekText)
        {
            //语音合成开始
            session_id = msc.MSCDLL.QTTSSessionBegin(offline_session_begin_params, ref err_code);

            if (err_code != (int) msc.Errors.MSP_SUCCESS)
            {
                Debug.LogError("初始化语音合成失败，错误信息：" + err_code);
                return;
            }

            //语音合成设置文本
            err_code = msc.MSCDLL.QTTSTextPut(session_id, speekText, (uint) Encoding.Default.GetByteCount(speekText),
                                              string.Empty);
            if (err_code != (int) msc.Errors.MSP_SUCCESS)
            {
                Debug.LogError("向服务器发送数据失败，错误信息：" + err_code);
                return;
            }

            uint audio_len    = 0;
            var  synth_status = msc.SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
            var  memoryStream = new MemoryStream();
            memoryStream.Write(new byte[44], 0, 44);
            while (true)
            {
                var source = msc.MSCDLL.QTTSAudioGet(session_id, ref audio_len, ref synth_status, ref err_code);
                var array  = new byte[audio_len];
                if (audio_len > 0) Marshal.Copy(source, array, 0, (int) audio_len);
                memoryStream.Write(array, 0, array.Length);
                Thread.Sleep(100);
                if (synth_status == msc.SynthStatus.MSP_TTS_FLAG_DATA_END || err_code != (int) msc.Errors.MSP_SUCCESS)
                    break;
            }

            err_code = msc.MSCDLL.QTTSSessionEnd(session_id, "");
            if (err_code != (int) msc.Errors.MSP_SUCCESS)
            {
                Debug.LogError("会话结束失败！错误信息: " + err_code);
                return;
            }

            var header     = getWave_Header((int) memoryStream.Length - 44); //创建wav文件头
            var headerByte = StructToBytes(header);                          //把文件头结构转化为字节数组
            memoryStream.Position = 0;                                       //定位到文件头
            memoryStream.Write(headerByte, 0, headerByte.Length);            //写入文件头
            bytes = memoryStream.ToArray();
            memoryStream.Close();
            if (Application.streamingAssetsPath + "/" + name + ".wav" != null)
            {
                if (File.Exists(Application.streamingAssetsPath + "/" + name + ".wav"))
                    File.Delete(Application.streamingAssetsPath + "/" + name + ".wav");
                File.WriteAllBytes(Application.streamingAssetsPath + "/" + name + ".wav", bytes);
                StartCoroutine(OnAudioLoadAndPaly(Application.streamingAssetsPath + "/" + name + ".wav", AudioType.WAV,
                                                  gameObject.GetComponent<AudioSource>()));
            }

            Debug.Log("合成结束成功");
        }


        /// <summary>
        ///     结构体转字符串
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        private byte[] StructToBytes(object structure)
        {
            var    num    = Marshal.SizeOf(structure);
            var    intPtr = Marshal.AllocHGlobal(num);
            byte[] result;
            try
            {
                Marshal.StructureToPtr(structure, intPtr, false);
                var array = new byte[num];
                Marshal.Copy(intPtr, array, 0, num);
                result = array;
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }

            return result;
        }

        /// <summary>
        ///     结构体初始化赋值
        /// </summary>
        /// <param name="data_len"></param>
        /// <returns></returns>
        private WAVE_Header getWave_Header(int data_len)
        {
            return new WAVE_Header
                   {
                       RIFF_ID           = 1179011410,
                       File_Size         = data_len + 36,
                       RIFF_Type         = 1163280727,
                       FMT_ID            = 544501094,
                       FMT_Size          = 16,
                       FMT_Tag           = 1,
                       FMT_Channel       = 1,
                       FMT_SamplesPerSec = 16000,
                       AvgBytesPerSec    = 32000,
                       BlockAlign        = 2,
                       BitsPerSample     = 16,
                       DATA_ID           = 1635017060,
                       DATA_Size         = data_len
                   };
        }

        /// <summary>
        ///     UnityWebRequest 加载音频播放
        /// </summary>
        /// <param name="url">路径</param>
        /// <param name="type">音频格式</param>
        /// <param name="audio">音频</param>
        /// <returns></returns>
        public IEnumerator OnAudioLoadAndPaly(string url, AudioType type, AudioSource audio)
        {
            var www = UnityWebRequestMultimedia.GetAudioClip(url, type);
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(www);
                audio.clip = clip;
                audio.Play();
            }
        }

        /// <summary>
        ///     语音音频头
        /// </summary>
        private struct WAVE_Header
        {
            public int    RIFF_ID;
            public int    File_Size;
            public int    RIFF_Type;
            public int    FMT_ID;
            public int    FMT_Size;
            public short  FMT_Tag;
            public ushort FMT_Channel;
            public int    FMT_SamplesPerSec;
            public int    AvgBytesPerSec;
            public ushort BlockAlign;
            public ushort BitsPerSample;
            public int    DATA_ID;
            public int    DATA_Size;
        }
    }
}