/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  public class RotateCube : MonoBehaviour
  {
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      // Rotate the object around its local X axis at 100 degree per second
      transform.Rotate(Vector3.right * Time.deltaTime * 100);

      // ...also rotate around the World's Y axis
      transform.Rotate(Vector3.up * Time.deltaTime * 100, Space.World);
    }
  }
}