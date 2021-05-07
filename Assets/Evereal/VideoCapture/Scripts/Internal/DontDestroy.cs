/* Copyright (c) 2020-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  public class DontDestroy : MonoBehaviour
  {
    void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }
  }
}