using CustomPostProcessing;
using UnityEngine;

namespace CustomCamera
{
    /// <summary>
    /// 相机图像混合
    /// </summary>
    public class CameraLayerMixer : CustomPostProcessingBase
    {
        public CameraMixer cameraMixer;

        /// <summary>
        ///     开启Layer0
        /// </summary>
        public bool enableLayer0;

        /// <summary>
        ///     开启layer1
        /// </summary>
        public bool enableLayer1;

        public  LayerCamera   layerCamera;
        public  Shader        mixShader;
        private Material      _mixMaterial;
        private RenderTexture _renderResultRT;

        private Material MixMaterial
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

        /// <summary>
        ///     抓取渲染结果
        /// </summary>
        /// <returns></returns>
        public RenderTexture GetRenderResult()
        {
            return _renderResultRT;
        }
    }
}