using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManagers
{
    /// <summary>
    ///     全局信息传递
    /// </summary>
    public class GlobalTransfer : MonoBehaviour
    {
        [SerializeField] private float _audioVolume = .5f;

        /// <summary>
        ///     难度
        /// </summary>
        public Difficulty difficulty;

        /// <summary>
        ///     问题名称
        /// </summary>
        public string sceneName;

        public string nextScene;

        public static GlobalTransfer getGlobalTransfer { get; private set; }

        /// <summary>
        ///     获取音量
        /// </summary>
        public float audioVolume
        {
            get => _audioVolume;
            set
            {
                _audioVolume = Mathf.Clamp01(value);
                GameManager.getGameManager.SetAudioVolume();
            }
        }

        private void Awake()
        {
            getGlobalTransfer = this;
            DontDestroyOnLoad(gameObject);
        }



        /// <summary>
        /// 在Loading场景中加载新场景
        /// </summary>
        /// <param name="nextLoadSceneName"></param>
        public void LoadSceneInLoadingScene(string nextLoadSceneName)
        {
            this.nextScene = nextLoadSceneName;
            SceneManager.LoadSceneAsync("Loading");
        }
    }
}