using System.Collections.Generic;
using GameManagers;
using UnityEngine;

namespace CustomUI.Quiz
{
    public class QuizStarsGroupUI : MonoBehaviour
    {
        public List<QuizStarUI> quizStarUis = new List<QuizStarUI>();
        public int              _starCount;

        public GlobalTimer globalTimer;

        public int starCount
        {
            get => _starCount;
            set
            {
                _starCount = Mathf.Clamp(value, 0, quizStarUis.Count);
                ShowStars();
            }
        }

        // private void Start()
        // {
        //     globalTimer = GameManager.GetGameManager.globalTimer;
        // }

        public void CalculateSuccessStars()
        {
            var step            = 1f                / quizStarUis.Count;
            var timeCostPercent = globalTimer.timer / globalTimer.countDownTime;
            starCount = quizStarUis.Count - (int) (timeCostPercent / step);
        }

        public void ShowStars()
        {
            for (var i = 0; i < quizStarUis.Count; i++) quizStarUis[i].isSet = i + 1 <= starCount;
        }
    }
}