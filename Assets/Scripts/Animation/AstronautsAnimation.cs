using System;
using System.Collections;
using CustomUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Animation
{
    public class AstronautsAnimation : MonoBehaviour
    {
        public Animator   astronautsAnimator;
        public float      boredTimeMin        = 3;
        public float      boredTimeMax        = 6;
        public int        boredAnimationCount = 5;
        public int        pointAnimationCount = 3;
        public SelectorUI selectorUI;

        private bool _isBored;
        private bool _isPoint;

        private void Start()
        {
            StartCoroutine(WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
        }

        private void Update()
        {
            if (selectorUI != null)
            {
                if (!selectorUI.isLocked && _isPoint)
                {
                    _isPoint = false;
                    astronautsAnimator.ResetTrigger("Point");
                }

                if (selectorUI.isLocked && !_isPoint)
                {
                    // astronautsAnimator.state
                    _isBored = false;
                    _isPoint = true;
                    astronautsAnimator.SetInteger("PointType", Random.Range(0, pointAnimationCount));
                    astronautsAnimator.SetTrigger("Point");
                    return;
                }
            }

            if (astronautsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Default"))
            {
                if (_isBored)
                {
                    _isBored = false;
                    StartCoroutine(WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                }
                else
                {
                    StartCoroutine(WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                }
            }
        }

        private IEnumerator WaitForBored(float time)
        {
            yield return new WaitForSeconds(time);
            _isBored = true;
            astronautsAnimator.SetInteger("BoredType", Random.Range(0, boredAnimationCount));
            astronautsAnimator.SetTrigger("Bored");
        }

        private IEnumerator WaitForAnimation(AnimationType animationType, float time)
        {
            yield return new WaitForSeconds(time);
            switch (animationType)
            {
                case AnimationType.Bored:
                    _isBored = false;
                    StartCoroutine(WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                    break;
                case AnimationType.Point:
                    _isPoint = false;
                    StartCoroutine(WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null);
            }
        }
    }
}