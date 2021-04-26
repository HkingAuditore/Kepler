using UnityEngine;

namespace Quiz
{
    /// <summary>
    /// 问题传输（已弃用）
    /// </summary>
    public class Transformer : MonoBehaviour
    {
        public static Transformer GetTransformer { get; private set; }

        private void Awake()
        {
            GetTransformer = this;
        }


        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}