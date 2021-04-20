using System;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class BloomEffect : MonoBehaviour
{
    private const int BoxDownPrefilterPass = 0;
    private const int BoxDownPass          = 1;
    private const int BoxUpPass            = 2;
    private const int ApplyBloomPass       = 3;
    private const int DebugBloomPass       = 4;

    public Shader bloomShader;

    public bool debug;

    [Range(0, 10)] public float intensity = 1;

    [Range(1, 16)] public int iterations = 4;

    [Range(0, 1)] public float softThreshold = 0.5f;

    [Range(0, 10)] public float threshold = 1;

    private readonly RenderTexture[] textures = new RenderTexture[16];

    [NonSerialized] private Material bloom;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (bloom == null)
        {
            bloom           = new Material(bloomShader);
            bloom.hideFlags = HideFlags.HideAndDontSave;
        }

        var     knee = threshold * softThreshold;
        Vector4 filter;
        filter.x = threshold;
        filter.y = filter.x - knee;
        filter.z = 2f    * knee;
        filter.w = 0.25f / (knee + 0.00001f);
        bloom.SetVector("_Filter", filter);
        bloom.SetFloat("_Intensity", Mathf.GammaToLinearSpace(intensity));

        var width  = source.width  / 2;
        var height = source.height / 2;
        var format = source.format;

        var currentDestination = textures[0] =
            RenderTexture.GetTemporary(width, height, 0, format);
        Graphics.Blit(source, currentDestination, bloom, BoxDownPrefilterPass);
        var currentSource = currentDestination;

        var i = 1;
        for (; i < iterations; i++)
        {
            width  /= 2;
            height /= 2;
            if (height < 2) break;
            currentDestination = textures[i] =
                RenderTexture.GetTemporary(width, height, 0, format);
            Graphics.Blit(currentSource, currentDestination, bloom, BoxDownPass);
            currentSource = currentDestination;
        }

        for (i -= 2; i >= 0; i--)
        {
            currentDestination = textures[i];
            textures[i]        = null;
            Graphics.Blit(currentSource, currentDestination, bloom, BoxUpPass);
            RenderTexture.ReleaseTemporary(currentSource);
            currentSource = currentDestination;
        }

        if (debug)
        {
            Graphics.Blit(currentSource, destination, bloom, DebugBloomPass);
        }
        else
        {
            bloom.SetTexture("_SourceTex", source);
            Graphics.Blit(currentSource, destination, bloom, ApplyBloomPass);
        }

        RenderTexture.ReleaseTemporary(currentSource);
    }
}