using UnityEngine;
using UnityEngine.UI;

namespace LeTai.Asset.TranslucentImage.Demo
{
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        private const float  fpsMeasurePeriod = 0.5f;
        private       string display          = "{0} FPS";
        private       int    m_CurrentFps;
        private       int    m_FpsAccumulator;
        private       float  m_FpsNextPeriod;
        private       Text   m_Text;


        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            m_Text          = GetComponent<Text>();
            display         = m_Text.text;
        }


        private void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps     =  (int) (m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator =  0;
                m_FpsNextPeriod  += fpsMeasurePeriod;
                m_Text.text      =  string.Format(display, m_CurrentFps);
            }
        }
    }
}