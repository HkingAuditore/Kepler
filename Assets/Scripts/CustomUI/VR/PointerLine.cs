using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerLine : MonoBehaviour
{
    public LineRenderer lineRenderer;

    private void Update()
    {
        lineRenderer.SetPositions(new Vector3[]
                                  {
                                      this.transform.position,
                                      this.transform.position + this.transform.forward * 1000
                                  });
    }
}
