using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPostProcessing
{

    public class OutlineCatcher : CustomPostProcessingBase,IRenderTexOuter
    {
        public Camera outlineCamera;
        
        public Shader outlineShader;
        private RenderTexture _outlineTexture;

        private Material _outlineMaterial;
        public Material OutlineMaterial
        {
            get
            {
                _outlineMaterial = GenerateMaterial(outlineShader,ref _outlineMaterial);
                return _outlineMaterial;
            }
        }
        
        
        [Header("Material Setting")]
        [Range(0,1)]
        public float edgeOnly = 1f;
        [Range(0.3f,1)]
        public float edgeSize = 0.5f;
        public Color edgeColor = Color.black;
        public Color backgroundColor = Color.white;

        #region 选择逻辑

        // private readonly List<GameObject> _selectedObjects = new List<GameObject>();
        public void AddTarget(GameObject target)
        {
            target.layer = LayerMask.NameToLayer("Outline");
        }

        public void RemoveTarget(GameObject target)
        {
            target.layer = LayerMask.NameToLayer("Default");
        }

        public void SetOutline()
        {
            
        }

        #endregion



        private void OnEnable()
        {
            outlineCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        private void Update()
        {
            outlineCamera.orthographicSize = mainCamera.orthographicSize;
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (OutlineMaterial != null)
            {
                OutlineMaterial.SetFloat("_EdgeOnly", edgeOnly);
                OutlineMaterial.SetFloat("_EdgeSize", edgeSize);
                OutlineMaterial.SetColor("_EdgeColor", edgeColor);
                OutlineMaterial.SetColor("_BackgroundColor", backgroundColor);
                Graphics.Blit(src, dest, OutlineMaterial);
                if (_renderResultRT == null)
                {
                    _renderResultRT = RenderTexture.GetTemporary(dest.width, dest.height);
                }
                Graphics.Blit(src, _renderResultRT, OutlineMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
                _renderResultRT = dest;
            }
        }
        
        private void OnDestroy()
        {
            if (_renderResultRT != null)
            {
                RenderTexture.ReleaseTemporary(_renderResultRT);
            }
        }

        private RenderTexture _renderResultRT;
        public RenderTexture GetRenderResult() => _renderResultRT;

    }
}