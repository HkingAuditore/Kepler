using System;
using UnityEngine;

namespace LeTai.Asset.TranslucentImage.Demo
{
    public class AutoMoveAndRotate : MonoBehaviour
    {
        public  Vector3andSpace moveUnitsPerSecond;
        public  Vector3andSpace rotateDegreesPerSecond;
        public  bool            ignoreTimescale, lateUpdate;
        private float           m_LastRealTime;


        private void Start()
        {
            m_LastRealTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (!lateUpdate)
                DoWork();
        }

        private void LateUpdate()
        {
            if (lateUpdate)
                DoWork();
        }

        private void DoWork()
        {
            var deltaTime = Time.deltaTime;
            if (ignoreTimescale)
            {
                deltaTime      = Time.realtimeSinceStartup - m_LastRealTime;
                m_LastRealTime = Time.realtimeSinceStartup;
            }

            transform.Translate(moveUnitsPerSecond.value  * deltaTime, moveUnitsPerSecond.space);
            transform.Rotate(rotateDegreesPerSecond.value * deltaTime, moveUnitsPerSecond.space);
        }


        [Serializable]
        public class Vector3andSpace
        {
            public Vector3 value;
            public Space   space = Space.Self;
        }
    }
}