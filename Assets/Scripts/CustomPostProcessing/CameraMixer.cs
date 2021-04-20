using UnityEngine;
using UnityEngine.Serialization;

namespace CustomPostProcessing
{
    public class CameraMixer : CustomPostProcessingBase
    {
        [FormerlySerializedAs("_mixMaterial")] public Material mixMaterial;

        [FormerlySerializedAs("_renderResultRT")]
        public RenderTexture renderResultRT;

        /// <summary>
        ///     边界颜色
        /// </summary>
        [Header("Material Setting")] public Color edgeColor = Color.white;

        public Shader         mixShader;
        public OutlineCatcher renderTexOuter;
        public RenderTexture  renderTexture;


        private Material MixMaterial
        {
            get
            {
                mixMaterial = GenerateMaterial(mixShader, ref mixMaterial);
                return mixMaterial;
            }
        }

        private void Start()
        {
            MixMaterial.EnableKeyword("MIXTEX0");
            MixMaterial.DisableKeyword("MIXTEX1");
            MixMaterial.DisableKeyword("MIXTEX2");
            MixMaterial.SetTexture("MixTex0", renderTexOuter.GetRenderResult());

            // MixMaterial.SetTexture("MixTex1", layerCamera.GetRenderResult());
            // MixMaterial.SetTexture("MixTex", renderTexture);
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (MixMaterial != null)
            {
                // Debug.Log(renderTexOuter.GetRenderResult());
                MixMaterial.SetTexture("_MixTex0", renderTexOuter.GetRenderResult());
                // MixMaterial.SetTexture("_MixTex1", layerCamera.GetRenderResult());
                MixMaterial.SetColor("_EdgeColor", edgeColor);
                // MixMaterial.SetTexture("MixTex", renderTexture);
                if (renderResultRT == null) renderResultRT = RenderTexture.GetTemporary(dest.width, dest.height);

                Graphics.Blit(src, dest,           MixMaterial);
                Graphics.Blit(src, renderResultRT, MixMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
                Graphics.Blit(src, renderResultRT);
            }
        }

        /// <summary>
        ///     抓取渲染结果
        /// </summary>
        /// <returns></returns>
        public RenderTexture GetRenderResult()
        {
            return renderResultRT;
        }
    }
}