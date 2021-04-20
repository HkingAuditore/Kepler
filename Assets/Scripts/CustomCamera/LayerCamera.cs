using CustomPostProcessing;
using UnityEngine;
using UnityEngine.Serialization;

namespace CustomCamera
{
    public class LayerCamera : MonoBehaviour, IRenderTexOuter
    {
        [FormerlySerializedAs("_renderResultRT")]
        public RenderTexture renderResultRT;

        private void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        }

        private void OnDestroy()
        {
            if (renderResultRT != null) RenderTexture.ReleaseTemporary(renderResultRT);
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest);
            if (renderResultRT == null) renderResultRT = RenderTexture.GetTemporary(dest.width, dest.height);
            Graphics.Blit(src, renderResultRT);
        }

        public RenderTexture GetRenderResult()
        {
            return renderResultRT;
        }
    }
}