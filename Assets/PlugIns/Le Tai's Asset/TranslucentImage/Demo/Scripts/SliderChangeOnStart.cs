using UnityEngine;
using UnityEngine.UI;

namespace LeTai.Asset.TranslucentImage.Demo
{
    [RequireComponent(typeof(Slider))]
    public class SliderChangeOnStart : MonoBehaviour
    {
        private Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();

            slider.value -= 1;
            slider.value += 1;
        }
    }
}