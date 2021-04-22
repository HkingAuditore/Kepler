using UnityEngine;

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
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}