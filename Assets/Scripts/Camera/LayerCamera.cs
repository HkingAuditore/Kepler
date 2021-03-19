﻿using System;
using System.Collections;
using System.Collections.Generic;
using CustomPostProcessing;
using UnityEngine;

public class LayerCamera : MonoBehaviour, IRenderTexOuter
{
    public RenderTexture _renderResultRT;

    private void OnEnable()
    {
        this.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnDestroy()
    {
        if (_renderResultRT != null) RenderTexture.ReleaseTemporary(_renderResultRT);
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
        if (_renderResultRT == null) _renderResultRT = RenderTexture.GetTemporary(dest.width, dest.height);
        Graphics.Blit(src, _renderResultRT);
    }
    public RenderTexture GetRenderResult()
    {
        return _renderResultRT;
    }

}

