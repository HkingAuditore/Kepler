using System;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    [Serializable]
    public class SizeModifier : SplineSampleModifier
    {
        public List<SizeKey> keys = new List<SizeKey>();

        public SizeModifier()
        {
            keys = new List<SizeKey>();
        }

        public override List<Key> GetKeys()
        {
            var output = new List<Key>();
            for (var i = 0; i < keys.Count; i++) output.Add(keys[i]);
            return output;
        }

        public override void SetKeys(List<Key> input)
        {
            keys = new List<SizeKey>();
            for (var i = 0; i < input.Count; i++)
            {
                input[i].modifier = this;
                keys.Add((SizeKey) input[i]);
            }
        }

        public void AddKey(double f, double t)
        {
            keys.Add(new SizeKey(f, t, this));
        }

        public override void Apply(SplineSample result)
        {
            if (keys.Count == 0) return;
            base.Apply(result);
            for (var i = 0; i < keys.Count; i++) result.size += keys[i].Evaluate(result.percent) * keys[i].size;
        }

        [Serializable]
        public class SizeKey : Key
        {
            public float size;

            public SizeKey(double f, double t, SizeModifier modifier) : base(f, t, modifier)
            {
            }
        }
    }
}