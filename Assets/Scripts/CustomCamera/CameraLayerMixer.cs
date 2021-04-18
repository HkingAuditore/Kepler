using CustomPostProcessing;
using UnityEngine;

namespace CustomCamera
{
    public class CameraLayerMixer : CustomPostProcessingBase
    {
        public CameraMixer cameraMixer;
        public LayerCamera layerCamera;
        public bool        enableLayer0;
        public bool        enableLayer1;

        public Shader mixShader;

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
            MixMaterial.DisableKeyword("MIXTEX0");

            if (enableLayer0)
                MixMaterial.EnableKeyword("MIXTEX1");
            else
                MixMaterial.DisableKeyword("MIXTEX1");
            if (enableLayer1)
                MixMaterial.EnableKeyword("MIXTEX2");
            else
                MixMaterial.DisableKeyword("MIXTEX2");

            MixMaterial.SetTexture("_MixTex1", cameraMixer.GetRenderResult());
            MixMaterial.SetTexture("_MixTex2", layerCamera.GetRenderResult());
            // MixMaterial.SetTexture("MixTex", renderTexture);
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (enableLayer0)
                MixMaterial.EnableKeyword("MIXTEX1");
            else
                MixMaterial.DisableKeyword("MIXTEX1");
            if (enableLayer1)
                MixMaterial.EnableKeyword("MIXTEX2");
            else
                MixMaterial.DisableKeyword("MIXTEX2");

            Debug.Log("1");
            if (MixMaterial != null)
            {
                if (_renderResultRT == null) _renderResultRT = RenderTexture.GetTemporary(dest.width, dest.height);
                // Debug.Log(renderTexOuter.GetRenderResult());
                MixMaterial.SetTexture("_MixTex1", cameraMixer.GetRenderResult());
                MixMaterial.SetTexture("_MixTex2", layerCamera.GetRenderResult());
                // MixMaterial.SetTexture("MixTex", renderTexture);
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