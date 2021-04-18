using System;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class DepthOfFieldEffect : MonoBehaviour
{
    private const int circleOfConfusionPass = 0;
    private const int preFilterPass         = 1;
    private const int bokehPass             = 2;
    private const int postFilterPass        = 3;
    private const int combinePass           = 4;

    [Range(0.1f, 100f)] public float focusDistance = 10f;

    [Range(0.1f, 10f)] public float focusRange = 3f;

    [Range(1f, 10f)] public float bokehRadius = 4f;

    // [HideInInspector]
    public Shader dofShader;

    [NonSerialized] private Material dofMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (dofMaterial == null)
        {
            dofMaterial           = new Material(dofShader);
            dofMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        dofMaterial.SetFloat("_BokehRadius",   bokehRadius);
        dofMaterial.SetFloat("_FocusDistance", focusDistance);
        dofMaterial.SetFloat("_FocusRange",    focusRange);

        var coc = RenderTexture.GetTemporary(
                                             source.width, source.height, 0,
                                             RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear
                                            );

        var width  = source.width  / 2;
        var height = source.height / 2;
        var format = source.format;
        var dof0   = RenderTexture.GetTemporary(width, height, 0, format);
        var dof1   = RenderTexture.GetTemporary(width, height, 0, format);

        dofMaterial.SetTexture("_CoCTex", coc);
        dofMaterial.SetTexture("_DoFTex", dof0);

        Graphics.Blit(source, coc,         dofMaterial, circleOfConfusionPass);
        Graphics.Blit(source, dof0,        dofMaterial, preFilterPass);
        Graphics.Blit(dof0,   dof1,        dofMaterial, bokehPass);
        Graphics.Blit(dof1,   dof0,        dofMaterial, postFilterPass);
        Graphics.Blit(source, destination, dofMaterial, combinePass);

        RenderTexture.ReleaseTemporary(coc);
        RenderTexture.ReleaseTemporary(dof0);
        RenderTexture.ReleaseTemporary(dof1);
    }
}