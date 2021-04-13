using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        animator = this.GetComponent<Animator>();
        PlayAnimation();
    }

    public void PlayAnimation()
    {
        this.GetComponent<Animator>().SetBool("StarSet", isSet);
        this.GetComponent<Animator>().SetBool("PreSet", true);
    }
}
