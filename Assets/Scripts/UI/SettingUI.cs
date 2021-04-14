using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public Slider      audioSlider;
    public AudioSource bgmSource;
    public void ChangeAudioVolume()
    {
        GlobalTransfer.getGlobalTransfer.audioVolume = audioSlider.value * .1f;
    }

    private void Start()
    {
        audioSlider.value = Mathf.Round(bgmSource.volume * 10);
    }

    public void ClosePanel()
    {
        this.gameObject.SetActive(false);
    }
}
