using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GlobalTimer : MonoBehaviour
{
    public float countDownTime;

    public UnityEvent startCountDownEvent  = new UnityEvent();
    public UnityEvent countingDownEvent    = new UnityEvent();
    public UnityEvent countingDownEndEvent = new UnityEvent();
    
    private bool         _isCountingDown = false;
    private bool         _isCountDownEnd = false;
    private bool         _isPausing = false;

    public bool isPausing
    {
        get => _isPausing;
        set => _isPausing = value;
    }

    public bool isCountDownEnd
    {
        get => _isCountDownEnd;
        set
        {
            _isCountDownEnd = value;
            countingDownEndEvent.Invoke();
        }
    }

    public bool isCountingDown
    {
        get => _isCountingDown;
        set => _isCountingDown = value;
    }

    public float timer
    {
        get => _timer;
        set => _timer = value;
    }

    private float _timer = 0f;

    public void StartCounting()
    {
        isCountingDown = true;
        startCountDownEvent.Invoke();
    }
    
    private void Update()
    {
        if(isCountingDown && !isPausing)
        {
            timer += Time.deltaTime;
            if (timer >= countDownTime)
            {
                isCountingDown = false;
                isCountDownEnd = true;
                
                return;
            }
            countingDownEvent.Invoke();
        }
        
    }

    public void Reset()
    {
        this.timer     = 0f;
        isCountingDown = false;
        isCountDownEnd = false;

    }



}
