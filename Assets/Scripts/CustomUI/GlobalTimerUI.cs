using GameManagers;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class GlobalTimerUI : MonoBehaviour
    {
        public  Text        timerText;
        private GlobalTimer _globalTimer;

        private void Start()
        {
            _globalTimer = GameManager.getGameManager.globalTimer;
        }

        private void Update()
        {
            // float remainTime = _globalTimer.countDownTime - _globalTimer.timer;
            // remainTime     = remainTime > 0 ? remainTime : 0;
            timerText.text = _globalTimer.timer.ToString("f2") + "s";
        }
    }
}