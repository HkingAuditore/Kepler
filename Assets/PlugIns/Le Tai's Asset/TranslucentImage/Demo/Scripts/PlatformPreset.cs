using System;
using UnityEngine;
using UnityEngine.UI;

namespace LeTai.Asset.TranslucentImage.Demo
{
    public class PlatformPreset : MonoBehaviour
    {
        public Preset[] presets;

        // Use this for initialization
        private void Start()
        {
            var sizeSlider         = GameObject.Find("Size Slider").GetComponent<Slider>();
            var iterationSlider    = GameObject.Find("Iteration Slider").GetComponent<Slider>();
            var downsampleSlider   = GameObject.Find("Downsample Slider").GetComponent<Slider>();
            var maxUpdateRateField = GameObject.Find("Max update rate Slider").GetComponent<Slider>();

            foreach (var preset in presets)
                if (preset.platform == Application.platform)
                {
                    sizeSlider.value         = preset.size;
                    iterationSlider.value    = preset.iteration;
                    downsampleSlider.value   = preset.downsample;
                    maxUpdateRateField.value = preset.maxUpdateRate;
                }
        }
    }

    [Serializable]
    public struct Preset
    {
        public RuntimePlatform platform;
        public float           size;
        public int             iteration;
        public int             downsample;
        public float           maxUpdateRate;
    }
}