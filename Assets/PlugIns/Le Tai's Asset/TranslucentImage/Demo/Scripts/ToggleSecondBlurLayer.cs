using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LeTai.Asset.TranslucentImage.Demo
{
    public class ToggleSecondBlurLayer : MonoBehaviour
    {
        public ChangeBlurConfig changer;

        public Slider updateRateInput;

        private void Start()
        {
            StartCoroutine(DisableSource());
        }

        private IEnumerator DisableSource()
        {
            yield return null;
            changer.SetUpdateRate(0);
        }

        public void Toggle()
        {
            if (Mathf.Approximately(changer.GetUpdateRate(), 0))
                changer.SetUpdateRate(updateRateInput.value);
            else
                changer.SetUpdateRate(0);
        }
    }
}