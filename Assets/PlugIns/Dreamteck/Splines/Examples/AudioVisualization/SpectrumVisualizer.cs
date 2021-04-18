using UnityEngine;

namespace Dreamteck.Splines.Examples
{
    public class SpectrumVisualizer : MonoBehaviour
    {
        public int samples = 1024;

        [Tooltip("The starting percent of the spectrum. 0 is 20Hz and 1 is 20KHz")] [Range(0f, 1f)]
        public float minSpectrumRange;

        [Tooltip("The ending percent of the spectrum. 0 is 20Hz and 1 is 20KHz")] [Range(0f, 1f)]
        public float maxSpectrumRange = 1f;

        public  float          increaseSpeed = 50f;
        public  float          decreaseSpeed = 10f;
        public  float          maxOffset     = 10f;
        public  AudioSource    source;
        public  AnimationCurve spectrumMultiply; //lower frequencies have bigger values, this is used to even the values
        private SplineComputer computer;
        private Vector3[]      positions;
        private float[]        spectrumLerp;


        // Use this for initialization
        private void Start()
        {
            if (source == null) source = GetComponent<AudioSource>();
            computer = GetComponent<SplineComputer>();
            var points = computer.GetPoints();
            positions = new Vector3[points.Length];
            for (var i = 0; i < points.Length; i++) positions[i] = points[i].position;
            spectrumLerp = new float[points.Length];
        }

        // Update is called once per frame
        private void Update()
        {
            var left  = new float[samples];
            var right = new float[samples];
            source.GetSpectrumData(left,  0, FFTWindow.Hanning);
            source.GetSpectrumData(right, 1, FFTWindow.Hanning);
            var spectrum                                          = new float[left.Length];
            for (var i = 0; i < spectrum.Length; i++) spectrum[i] = (left[i] + right[i]) / 2f;
            var points                                            = computer.GetPoints();
            var samplesPerPoint =
                Mathf.FloorToInt(spectrum.Length / points.Length * (maxSpectrumRange - minSpectrumRange));
            var spectrumIndexStart = Mathf.FloorToInt((spectrum.Length - 1) * minSpectrumRange);
            for (var i = 0; i < points.Length; i++)
            {
                var avg                                       = 0f;
                for (var n = 0; n < samplesPerPoint; n++) avg += spectrum[spectrumIndexStart + samplesPerPoint * i + n];
                avg /= samplesPerPoint;
                if (avg > spectrumLerp[i])
                    spectrumLerp[i]  = Mathf.Lerp(spectrumLerp[i], avg, Time.deltaTime * increaseSpeed);
                else spectrumLerp[i] = Mathf.Lerp(spectrumLerp[i], avg, Time.deltaTime * decreaseSpeed);

                var percent = (float) i / (points.Length - 1);
                points[i].position = positions[i] +
                                     Vector3.up * maxOffset * spectrumLerp[i] * spectrumMultiply.Evaluate(percent);
            }

            computer.SetPoints(points);
        }
    }
}