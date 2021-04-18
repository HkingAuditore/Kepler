using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
    [Serializable]
    public class MeshScaleModifier : SplineSampleModifier
    {
        public List<ScaleKey> keys = new List<ScaleKey>();

        public MeshScaleModifier()
        {
            keys = new List<ScaleKey>();
        }

        public override List<Key> GetKeys()
        {
            var output = new List<Key>();
            for (var i = 0; i < keys.Count; i++) output.Add(keys[i]);
            return output;
        }

        public override void SetKeys(List<Key> input)
        {
            keys = new List<ScaleKey>();
            for (var i = 0; i < input.Count; i++)
            {
                input[i].modifier = this;
                keys.Add((ScaleKey) input[i]);
            }
        }

        public void AddKey(double f, double t)
        {
            keys.Add(new ScaleKey(f, t, this));
        }

        public override void Apply(SplineSample result)
        {
            if (keys.Count == 0) return;
            for (var i = 0; i < keys.Count; i++)
                result.size += keys[i].Evaluate(result.percent) * keys[i].scale.magnitude;
        }

        public Vector2 GetScale(SplineSample sample)
        {
            var scale = Vector2.one;
            for (var i = 0; i < keys.Count; i++)
            {
                var lerp            = keys[i].Evaluate(sample.percent);
                var scaleMultiplier = Vector2.Lerp(Vector2.one, keys[i].scale, lerp);
                scale.x *= scaleMultiplier.x;
                scale.y *= scaleMultiplier.y;
            }

            return scale;
        }

        [Serializable]
        public class ScaleKey : Key
        {
            public Vector2 scale = Vector2.one;

            public ScaleKey(double f, double t, MeshScaleModifier modifier) : base(f, t, modifier)
            {
            }
        }
    }
}