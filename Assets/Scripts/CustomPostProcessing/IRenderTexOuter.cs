using UnityEngine;

namespace CustomPostProcessing
{
    /// <summary>
    /// RenderTexture输出接口
    /// </summary>
    public interface IRenderTexOuter
    {
        RenderTexture GetRenderResult();
    }
}