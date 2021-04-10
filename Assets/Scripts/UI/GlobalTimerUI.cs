using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalTimerUI : MonoBehaviour
{
    public  Text        timerText;
    private GlobalTimer _globalTimer;

    private void Start()
    {
        _globalTimer = GameManager.GetGameManager.globalTimer;
    }

    private void Update()
    {
        float remainTime = _globalTimer.countDownTime - _globalTimer.timer;
        remainTime = remainTime > 0 ? remainTime : 0;
        timerText.text = remainTime.ToString("f2") + "s";
    }
}
