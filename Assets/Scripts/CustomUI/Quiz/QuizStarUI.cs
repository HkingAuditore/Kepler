using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.Quiz
{
    public class QuizStarUI : MonoBehaviour
    {
        public Image    starBackground;
        public Image    starForward;
        public Animator animator;

        private bool _isSet;

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

        public void PlayAnimation()
        {
            GetComponent<Animator>().SetBool("StarSet", isSet);
            GetComponent<Animator>().SetBool("PreSet",  true);
        }
    }
}