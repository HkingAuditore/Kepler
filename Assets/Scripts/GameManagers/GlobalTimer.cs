using UnityEngine;
using UnityEngine.Events;

namespace GameManagers
{
    public class GlobalTimer : MonoBehaviour
    {
        /// <summary>
        ///     计时时间
        /// </summary>
        public float countDownTime;

        /// <summary>
        ///     倒计时完成事件
        /// </summary>
        public UnityEvent countingDownEndEvent = new UnityEvent();

        /// <summary>
        ///     倒计时执行中事件
        /// </summary>
        public UnityEvent countingDownEvent = new UnityEvent();

        /// <summary>
        ///     开始倒计时事件
        /// </summary>
        public UnityEvent startCountDownEvent = new UnityEvent();

        private bool _isCountDownEnd;

        /// <summary>
        ///     是否暂停
        /// </summary>
        public bool isPausing { get; set; } = false;

        private bool isCountDownEnd
        {
            get => _isCountDownEnd;
            set
            {
                _isCountDownEnd = value;
                countingDownEndEvent.Invoke();
            }
        }

        private bool isCountingDown { get; set; }

        /// <summary>
        ///     计时器
        /// </summary>
        public float timer { get; set; }

        /// <summary>
        ///     重置计时器
        /// </summary>
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

        /// <summary>
        ///     开始计时
        /// </summary>
        public void StartCounting()
        {
            isCountingDown = true;
            startCountDownEvent.Invoke();
        }
    }
}