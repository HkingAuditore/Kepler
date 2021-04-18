using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
    [Serializable]
    public class OffsetModifier : SplineSampleModifier
    {
        public List<OffsetKey> keys = new List<OffsetKey>();

        public OffsetModifier()
        {
            keys = new List<OffsetKey>();
        }

        public override List<Key> GetKeys()
        {
            var output = new List<Key>();
            for (var i = 0; i < keys.Count; i++) output.Add(keys[i]);
            return output;
        }

        public override void SetKeys(List<Key> input)
        {
            keys = new List<OffsetKey>();
            for (var i = 0; i < input.Count; i++) keys.Add((OffsetKey) input[i]);
            base.SetKeys(input);
        }

        public void AddKey(Vector2 offset, double f, double t)
        {
            keys.Add(new OffsetKey(offset, f, t, this));
        }

        public override void Apply(SplineSample result)
        {
            if (keys.Count == 0) return;
            base.Apply(result);
            var offset = Evaluate(result.percent);
            result.position += result.right * offset.x + result.up * offset.y;
        }

        private Vector2 Evaluate(double time)
        {
            if (keys.Count == 0) return Vector2.zero;
            var offset                                  = Vector2.zero;
            for (var i = 0; i < keys.Count; i++) offset += keys[i].offset * keys[i].Evaluate(time);
            return offset * blend;
        }

        [Serializable]
        public class OffsetKey : Key
        {
            public Vector2 offset = Vector2.zero;

            public OffsetKey(Vector2 o, double f, double t, OffsetModifier modifier) : base(f, t, modifier)
            {
                offset = o;
            }
        }
    }
}