using UnityEngine;

namespace Quiz
{
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