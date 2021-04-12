using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Animation
{
    enum AnimationType
    {
        Bored,
        Point
    }
    public class AstronautsAnimation : MonoBehaviour
    {
        public Animator   astronautsAnimator;
        public float      boredTimeMin        = 3;
        public float      boredTimeMax        = 6;
        public int        boredAnimationCount = 5;
        public int        pointAnimationCount = 3;
        public SelectorUI selectorUI;
    
        private bool _isBored = false;
        private bool _isPoint = false;
    
        private void Start()
        {
            StartCoroutine( WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
        }

        private void Update()
        {
            if(selectorUI !=null)
            {
                if (!selectorUI.isLocked && this._isPoint)
                {
                    _isPoint = false;
                    astronautsAnimator.ResetTrigger("Point");
                }

                if (selectorUI.isLocked && !this._isPoint)
                {
                    // astronautsAnimator.state
                    _isBored = false;
                    _isPoint = true;
                    astronautsAnimator.SetInteger("PointType", Random.Range(0, pointAnimationCount));
                    astronautsAnimator.SetTrigger("Point");
                    return;
                }
            }
            if (this.astronautsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Default"))
            {
                if (_isBored)
                {
                    _isBored = false;
                    StartCoroutine( WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                }
                else
                {
                    StartCoroutine( WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                }

            }
            
        }

        IEnumerator WaitForBored(float time)
        {
            yield return new WaitForSeconds(time);
            _isBored = true;
            astronautsAnimator.SetInteger("BoredType", Random.Range(0, boredAnimationCount));
            astronautsAnimator.SetTrigger("Bored");

        }
    
        IEnumerator WaitForAnimation(AnimationType animationType,float time)
        {
            yield return new WaitForSeconds(time);
            switch (animationType)
            {
                case AnimationType.Bored:
                    _isBored = false;
                    StartCoroutine( WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                    break;
                case AnimationType.Point:
                    _isPoint = false;
                    StartCoroutine( WaitForBored(Random.Range(boredTimeMin, boredTimeMax)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null);
            }

        }
    
    
    }
}