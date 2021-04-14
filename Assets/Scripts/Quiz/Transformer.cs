using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformer : MonoBehaviour
{
    public static Transformer GetTransformer { get; private set; }

    private void Awake()
    {
        GetTransformer = this;
    }



    private void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }
}
