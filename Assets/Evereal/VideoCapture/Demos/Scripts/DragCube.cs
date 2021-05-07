/* Copyright (c) 2019-present Evereal. All rights reserved. */

using UnityEngine;

namespace Evereal.VideoCapture
{
  public class DragCube : MonoBehaviour
  {
    /// <summary>
    /// Remove Blitter overlay to make it work
    /// </summary>
    private Vector3 moffset;
    private float mZCoord;

    void OnMouseDown()
    {
      if (!enabled)
        return;

      mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
      moffset = gameObject.transform.position - GetMouseWorldPos();

    }

    private Vector3 GetMouseWorldPos()
    {
      Vector3 mousePoint = Input.mousePosition;
      mousePoint.z = mZCoord;

      return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
      if (!enabled)
        return;

      transform.position = GetMouseWorldPos() + moffset;
    }
  }
}