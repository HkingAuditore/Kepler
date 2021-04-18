using System;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    [Serializable]
    public class FollowerSpeedModifier : SplineSampleModifier
    {
        public List<SpeedKey> keys = new List<SpeedKey>();

        public FollowerSpeedModifier()
        {
            keys = new List<SpeedKey>();
        }

        public override List<Key> GetKeys()
        {
            var output = new List<Key>();
            for (var i = 0; i < keys.Count; i++) output.Add(keys[i]);
            return output;
        }

        public override void SetKeys(List<Key> input)
        {
            keys = new List<SpeedKey>();
            for (var i = 0; i < input.Count; i++)
            {
                input[i].modifier = this;
                keys.Add((SpeedKey) input[i]);
            }
        }

        public void AddKey(double f, double t)
        {
            keys.Add(new SpeedKey(f, t, this));
        }

        public override void Apply(SplineSample result)
        {
        }

        public float GetSpeed(SplineSample sample)
        {
            var speed = 0f;
            for (var i = 0; i < keys.Count; i++)
            {
                var lerp = keys[i].Evaluate(sample.percent);
                speed += keys[i].speed * lerp;
            }

            return speed;
        }

        [Serializable]
        public class SpeedKey : Key
        {
            public float speed;

            public SpeedKey(double f, double t, FollowerSpeedModifier modifier) : base(f, t, modifier)
            {
            }
        }
    }
}