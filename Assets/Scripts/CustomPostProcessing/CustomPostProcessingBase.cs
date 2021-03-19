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

        private void Start()
        {
            mainCamera = GameManager.GetGameManager.mainCamera;
        }

        protected Material GenerateMaterial(Shader shader, ref Material targetMaterial)
        {
            Material nullMat = null;
            if (shader != null)
                if (shader.isSupported)
                {
                    if (targetMaterial == null) targetMaterial = new Material(shader);
                    return targetMaterial;
                }

            return nullMat;
        }
    }
}