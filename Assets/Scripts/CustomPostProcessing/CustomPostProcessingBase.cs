using UnityEngine;

namespace CustomPostProcessing
{
    /// <summary>
    /// 自定义后处理
    /// </summary>
    [ExecuteInEditMode]
    public class CustomPostProcessingBase : MonoBehaviour
    {
        /// <summary>
        ///     主相机
        /// </summary>
        public Camera mainCamera;

        // private void Start()
        // {
        //     mainCamera = GameManager.GetGameManager.mainCamera;
        // }

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