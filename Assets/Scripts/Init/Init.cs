using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Init
{
    /// <summary>
    /// Init场景管理
    /// </summary>
    public class Init : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("Main");
        }
    }
}