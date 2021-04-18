using UnityEngine;

namespace Dreamteck.Splines.Examples
{
    public class SetMaterialColor : MonoBehaviour
    {
        public  Color[]        colors;
        private SplineRenderer rend;

        private void Start()
        {
            rend = GetComponent<SplineRenderer>();
        }

        public void SetColor(int index)
        {
            if (!Application.isPlaying) return;
            rend.color = colors[index];
        }
    }
}