using System;
using System.Runtime.InteropServices;

namespace msc
{
    public class MSCDLL
    {
        #region 登录登出

        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int MSPLogin(string usr, string pwd, string parameters);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int MSPLogout();

        #endregion

        #region 语音识别

        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QISRSessionBegin(string grammarList, string _params, ref int errorCode);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRAudioWrite(IntPtr          sessionID,   byte[]       waveData, uint waveLen,
                                                AudioStatus     audioStatus, ref EpStatus epStatus,
                                                ref RecogStatus recogStatus);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QISRGetResult(IntPtr  sessionID, ref RecogStatus rsltStatus, int waitTime,
                                                  ref int errorCode);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRSessionEnd(IntPtr sessionID, string hints);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRBuildGrammar(IntPtr grammarType, string grammarContent, uint grammarLength,
                                                  string _params,     GrammarCallBack callback, IntPtr userData);


        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate int GrammarCallBack(int errorCode, string info, object udata);


        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QISRUploadData(string sessionID,  string  dataName, byte[] userData, uint lenght,
                                                   string paramValue, ref int errorCode);

        #endregion

        #region 语音唤醒

        //定义回调函数
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ivw_ntf_handler(string sessionID, int msg, int param1, int param2, IntPtr info,
                                            IntPtr userData);


        //调用 QIVWSessionBegin(...)开始一次语音唤醒
        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QIVWSessionBegin(string grammarList, string _params, ref int errorCode);


        //调用 QIVWAudioWrite(...) 分块写入音频数据
        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QIVWAudioWrite(string      sessionID, byte[] waveData, uint waveLen,
                                                AudioStatus audioStatus);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QIVWGetResInfo(string resPath, string resInfo, uint infoLen, string _params);


        //调用 QIVWRegisterNotify(...) 注册回调函数到msc。
        //如果唤醒成功，msc 调用回调函数通知唤醒成功息同时给出相应唤醒数据。如果出错，msc 调用回调函数给出错误信息
        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QIVWRegisterNotify(string                                                 sessionID,
                                                    [MarshalAs(UnmanagedType.FunctionPtr)] ivw_ntf_handler msgProcCb,
                                                    IntPtr                                                 userData);


        //调用 QIVWSessionEnd(...) 主动结束本次唤醒
        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QIVWSessionEnd(string sessionID, string hints);

        #endregion

        #region 语音合成

        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QTTSSessionBegin(string _params, ref int errorCode);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QTTSTextPut(IntPtr sessionID, string textString, uint textLen, string _params);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QTTSAudioGet(IntPtr  sessionID, ref uint audioLen, ref SynthStatus synthStatus,
                                                 ref int errorCode);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QTTSAudioInfo(IntPtr sessionID);


        [DllImport("msc_x64", CallingConvention = CallingConvention.StdCall)]
        public static extern int QTTSSessionEnd(IntPtr sessionID, string hints);

        #endregion
    }

    /*
    *  The enumeration MSPRecognizerStatus contains the recognition status
    *  MSP_REC_STATUS_SUCCESS				- successful recognition with partial results
    *  MSP_REC_STATUS_NO_MATCH				- recognition rejected
    *  MSP_REC_STATUS_INCOMPLETE			- recognizer needs more time to compute results
    *  MSP_REC_STATUS_NON_SPEECH_DETECTED	- discard status, no more in use
    *  MSP_REC_STATUS_SPEECH_DETECTED		- recognizer has detected audio, this is delayed status
    *  MSP_REC_STATUS_COMPLETE				- recognizer has return all result
    *  MSP_REC_STATUS_MAX_CPU_TIME			- CPU time limit exceeded
    *  MSP_REC_STATUS_MAX_SPEECH			- maximum speech length exceeded, partial results may be returned
    *  MSP_REC_STATUS_STOPPED				- recognition was stopped
    *  MSP_REC_STATUS_REJECTED				- recognizer rejected due to low confidence
    *  MSP_REC_STATUS_NO_SPEECH_FOUND		- recognizer still found no audio, this is delayed status
    */

    /* Synthesizing process flags */

    /* Handwriting process flags */

    /*ivw message type */

    /* Upload data process flags */
}