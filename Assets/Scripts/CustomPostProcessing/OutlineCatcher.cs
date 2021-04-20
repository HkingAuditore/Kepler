using UnityEngine;

namespace CustomPostProcessing
{
    public class OutlineCatcher : CustomPostProcessingBase, IRenderTexOuter
    {
        /// <summary>
        ///     背景颜色
        /// </summary>
        public Color backgroundColor = Color.white;

        /// <summary>
        ///     描边颜色
        /// </summary>
        public Color edgeColor = Color.black;

        /// <summary>
        ///     描边范围
        /// </summary>
        [Header("Material Setting")] [Range(0, 1)]
        public float edgeOnly = 1f;

        /// <summary>
        ///     描边尺寸
        /// </summary>
        [Range(0.3f, 1)] public float edgeSize = 0.5f;

        /// <summary>
        ///     描边相机
        /// </summary>
        public Camera outlineCamera;

        public  Shader        outlineShader;
        private Material      _outlineMaterial;
        private RenderTexture _outlineTexture;
        private RenderTexture _renderResultRT;

        private Material outlineMaterial
        {
            get
            {
                _outlineMaterial = GenerateMaterial(outlineShader, ref _outlineMaterial);
                return _outlineMaterial;
            }
        }

        private void Update()
        {
            outlineCamera.orthographicSize = mainCamera.orthographicSize;
        }


        private void OnEnable()
        {
            outlineCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        private void OnDestroy()
        {
            if (_renderResultRT != null) RenderTexture.ReleaseTemporary(_renderResultRT);
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (outlineMaterial != null)
            {
                outlineMaterial.SetFloat("_EdgeOnly", edgeOnly);
                outlineMaterial.SetFloat("_EdgeSize", edgeSize);
                outlineMaterial.SetColor("_EdgeColor",       edgeColor);
                outlineMaterial.SetColor("_BackgroundColor", backgroundColor);
                Graphics.Blit(src, dest, outlineMaterial);
                if (_renderResultRT == null) _renderResultRT = RenderTexture.GetTemporary(dest.width, dest.height);
                Graphics.Blit(src, _renderResultRT, outlineMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
                _renderResultRT = dest;
            }
        }

        public RenderTexture GetRenderResult()
        {
            return _renderResultRT;
        }

        #region 选择逻辑

        /// <summary>
        ///     新增描边对象
        /// </summary>
        /// <param name="target">描边对象</param>
        public void AddTarget(GameObject target)
        {
            target.layer = LayerMask.NameToLayer("Outline");
        }

        /// <summary>
        ///     移除描边对象
        /// </summary>
        /// <param name="target">
        ///     描边对象<</param>
        public void RemoveTarget(GameObject target)
        {
            target.layer = 23;
        }

        #endregion
    }
}