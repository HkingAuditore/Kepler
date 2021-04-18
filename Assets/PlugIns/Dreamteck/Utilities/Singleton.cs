using System.Linq;
using UnityEngine;

namespace Dreamteck
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static         T    _instance;
        [SerializeField] private bool _dontDestryOnLoad = true;
        [SerializeField] private bool _overrideInstance;

        public static T instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectsOfType<T>().FirstOrDefault();

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (_overrideInstance)
                {
                    Destroy(_instance.gameObject);
                    _instance = this as T;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                _instance = this as T;

                if (_dontDestryOnLoad) DontDestroyOnLoad(gameObject);
            }

            Init();
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this && !_overrideInstance) _instance = null;
        }

        protected virtual void Init()
        {
        }
    }
}