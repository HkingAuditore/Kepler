using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.Quiz
{
    public class QuizStarUI : MonoBehaviour
    {
        public  Animator animator;
        public  Image    starBackground;
        public  Image    starForward;
        private bool     _isSet;

        public bool isSet
        {
            get => _isSet;
            set
            {
                _isSet = value;
                PlayAnimation();
            }
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            GetComponent<Animator>().SetBool("StarSet", isSet);
            GetComponent<Animator>().SetBool("PreSet",  true);
        }
    }
}