
using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck;
using UnityEngine;

public enum EnumSpeaker
{
    /// <summary>
    /// 女声：晓燕
    /// </summary>
    xiaoyan,
}
public enum SpeakType
{
    /// <summary>
    /// 普通播放类型，顺序播放
    /// </summary>
    normal,
    /// <summary>
    /// 立刻播放类型，停止当前立即播放
    /// </summary>
    now,
}

public class Speaker : Singleton<Speaker>
{
    GameObject speakObj;
    XunFeiOffline xunFei;

    public string contentStr;

    /// <summary>
    /// 讯飞语音调用器
    /// </summary>
    /// <param name="content"></param>
    /// <param name="speaker"></param>
    /// <param name="speakType"></param>
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Speak(contentStr);
        }
    }

    public void Speak(string content, EnumSpeaker speaker = EnumSpeaker.xiaoyan, SpeakType speakType = SpeakType.normal)
    {
        if (!speakObj)
        {
            speakObj = new GameObject("Speaker");
            speakObj.AddComponent<AudioSource>();
            speakObj.AddComponent<AudioListener>();
            speakObj.AddComponent<XunFeiOffline>();
            xunFei = speakObj.GetComponent<XunFeiOffline>();
        }
        if (!xunFei)
        {
            xunFei = speakObj.GetComponent<XunFeiOffline>();
        }       
        xunFei.ToSpeak(speaker, content, speakType);



    }

}
