using UnityEngine;

namespace CustomPostProcessing
{
    public class CameraMixer : CustomPostProcessingBase
    {
        // public List<IRenderTexOuter> renderTexOuters = new List<IRenderTexOuter>();
        public OutlineCatcher renderTexOuter;
        // public LayerCamera    layerCamera;
        public RenderTexture  renderTexture;
        public Shader         mixShader;

        [Header("Material Setting")] public Color edgeColor = Color.white;

        public Material      _mixMaterial;
        public RenderTexture _renderResultRT;



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
                if (_renderResultRT == null) _renderResultRT = RenderTexture.GetTemporary(dest.width, dest.height);

                Graphics.Blit(src, dest,            MixMaterial);
                Graphics.Blit(src, _renderResultRT, MixMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
                Graphics.Blit(src, _renderResultRT);
            }
        }
        public RenderTexture GetRenderResult()
        {
            return _renderResultRT;
        }
    }
}