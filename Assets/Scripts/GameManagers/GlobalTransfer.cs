using UnityEngine;

namespace GameManagers
{
    [SerializeField]
    public enum Difficulty
    {
        Easy,
        Normal,
        Difficult,
        Real
    }

    public class GlobalTransfer : MonoBehaviour
    {
        [SerializeField] private float      _audioVolume = .5f;
        public                   string     quizName;
        public                   Difficulty difficulty;

        public static GlobalTransfer getGlobalTransfer { get; private set; }

        public float audioVolume
        {
            get => _audioVolume;
            set
            {
                _audioVolume = Mathf.Clamp01(value);
                GameManager.GetGameManager.SetAudioVolume();
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