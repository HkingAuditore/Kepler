using UnityEngine;

namespace CustomPostProcessing
{
    public interface IRenderTexOuter
    {
        RenderTexture GetRenderResult();
    }
    [ExecuteInEditMode]
    public class CustomPostProcessingBase : MonoBehaviour
    {
        protected Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        protected Material GenerateMaterial(Shader shader,ref Material targetMaterial)
        {
        
            Material nullMat = null;
            if (shader != null){
                if (shader.isSupported)
                {
                    if (targetMaterial == null)
                    {
                        targetMaterial = new Material(shader);
                    }
                    return targetMaterial;
                }
            }
            return nullMat;
        }

    }
}
