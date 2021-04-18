using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Users/Length Calculator")]
    public class LengthCalculator : SplineUser
    {
        [HideInInspector] public LengthEvent[] lengthEvents = new LengthEvent[0];

        [HideInInspector] public float idealLength = 1f;

        private float lastLength;

        public float length { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            length     = CalculateLength();
            lastLength = length;
            for (var i = 0; i < lengthEvents.Length; i++)
                if (lengthEvents[i].targetLength == length)
                    lengthEvents[i].onChange.Invoke();
        }

        protected override void Build()
        {
            base.Build();
            length = CalculateLength();
            if (lastLength != length)
            {
                for (var i = 0; i < lengthEvents.Length; i++) lengthEvents[i].Check(lastLength, length);
                lastLength = length;
            }
        }

        private void AddEvent(LengthEvent lengthEvent)
        {
            var newEvents = new LengthEvent[lengthEvents.Length + 1];
            lengthEvents.CopyTo(newEvents, 0);
            newEvents[newEvents.Length - 1] = lengthEvent;
            lengthEvents                    = newEvents;
        }

        [Serializable]
        public class LengthEvent
        {
            public enum Type
            {
                Growing,
                Shrinking,
                Both
            }

            public bool       enabled = true;
            public float      targetLength;
            public UnityEvent onChange = new UnityEvent();
            public Type       type     = Type.Both;

            public LengthEvent()
            {
            }

            public LengthEvent(Type t)
            {
                type = t;
            }

            public void Check(float fromLength, float toLength)
            {
                if (!enabled) return;
                var condition = false;
                switch (type)
                {
                    case Type.Growing:
                        condition = toLength >= targetLength && fromLength < targetLength;
                        break;
                    case Type.Shrinking:
                        condition = toLength <= targetLength && fromLength > targetLength;
                        break;
                    case Type.Both:
                        condition = toLength >= targetLength && fromLength < targetLength ||
                                    toLength <= targetLength && fromLength > targetLength;
                        break;
                }

                if (condition) onChange.Invoke();
            }
        }
    }
}