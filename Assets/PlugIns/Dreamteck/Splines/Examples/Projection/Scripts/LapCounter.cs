using UnityEngine;

namespace Dreamteck.Splines.Examples
{
    public class LapCounter : MonoBehaviour
    {
        public  TextMesh text;
        private int      currentLap;

        public void CountLap()
        {
            currentLap++;
            text.text = "LAP " + currentLap;
        }
    }
}