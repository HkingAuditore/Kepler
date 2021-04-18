using UnityEngine;
using UnityEngine.Events;

namespace GameManagers
{
    public class GlobalTimer : MonoBehaviour
    {
        public float countDownTime;

        public  UnityEvent startCountDownEvent  = new UnityEvent();
        public  UnityEvent countingDownEvent    = new UnityEvent();
        public  UnityEvent countingDownEndEvent = new UnityEvent();
        private bool       _isCountDownEnd;

        public bool isPausing { get; set; } = false;

        public bool isCountDownEnd
        {
            get => _isCountDownEnd;
            set
            {
                _isCountDownEnd = value;
                countingDownEndEvent.Invoke();
            }
        }

        public bool isCountingDown { get; set; }

        public float timer { get; set; }

        public void Reset()
        {
            timer          = 0f;
            isCountingDown = false;
            isCountDownEnd = false;
        }

        private void Update()
        {
            if (isCountingDown && !isPausing)
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

        public void StartCounting()
        {
            isCountingDown = true;
            startCountDownEvent.Invoke();
        }
    }
}