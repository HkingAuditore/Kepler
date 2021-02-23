using UnityEngine;

namespace CustomPostProcessing
{
    public class CameraMixer : CustomPostProcessingBase
    {
        // public List<IRenderTexOuter> renderTexOuters = new List<IRenderTexOuter>();
        public OutlineCatcher renderTexOuter;
        public RenderTexture renderTexture;
        public Shader mixShader;

        [Header("Material Setting")] public Color edgeColor = Color.white;

        private Material _mixMaterial;


        public Material MixMaterial
        {
            get
            {
                _mixMaterial = GenerateMaterial(mixShader, ref _mixMaterial);
                return _mixMaterial;
            }
        }

        private void Start()
        {
            MixMaterial.SetTexture("MixTex", renderTexOuter.GetRenderResult());
            // MixMaterial.SetTexture("MixTex", renderTexture);
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (MixMaterial != null)
            {
                // Debug.Log(renderTexOuter.GetRenderResult());
                MixMaterial.SetTexture("_MixTex", renderTexOuter.GetRenderResult());
                MixMaterial.SetColor("_EdgeColor", edgeColor);
                // MixMaterial.SetTexture("MixTex", renderTexture);
                Graphics.Blit(src, dest, MixMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }
    }
}