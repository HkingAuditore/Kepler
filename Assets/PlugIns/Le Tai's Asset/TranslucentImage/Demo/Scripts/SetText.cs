using UnityEngine;
using UnityEngine.UI;

namespace LeTai.Asset.TranslucentImage.Demo
{
    public class SetText : MonoBehaviour
    {
        public string format = "0.0";

        private Text text;

        // Use this for initialization
        private void Awake()
        {
            text = GetComponent<Text>();
        }

        public void Set(float value)
        {
            text.text = value.ToString(format);
        }
    }
}