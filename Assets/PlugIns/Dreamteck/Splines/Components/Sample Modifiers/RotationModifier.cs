using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
    [Serializable]
    public class RotationModifier : SplineSampleModifier
    {
        public List<RotationKey> keys = new List<RotationKey>();

        public RotationModifier()
        {
            keys = new List<RotationKey>();
        }

        public override List<Key> GetKeys()
        {
            var output = new List<Key>();
            for (var i = 0; i < keys.Count; i++) output.Add(keys[i]);
            return output;
        }

        public override void SetKeys(List<Key> input)
        {
            keys = new List<RotationKey>();
            for (var i = 0; i < input.Count; i++) keys.Add((RotationKey) input[i]);
            base.SetKeys(input);
        }

        public void AddKey(Vector3 rotation, double f, double t)
        {
            keys.Add(new RotationKey(rotation, f, t, this));
        }

        public override void Apply(SplineSample result)
        {
            if (keys.Count == 0) return;
            base.Apply(result);

            Quaternion offset = Quaternion.identity, look = result.rotation;
            for (var i = 0; i < keys.Count; i++)
                if (keys[i].useLookTarget && keys[i].target != null)
                {
                    var lookDir = Quaternion.LookRotation(keys[i].target.position - result.position);
                    look = Quaternion.Slerp(look, lookDir, keys[i].Evaluate(result.percent));
                }
                else
                {
                    var euler = Quaternion.Euler(keys[i].rotation.x, keys[i].rotation.y, keys[i].rotation.z);
                    offset = Quaternion.Slerp(offset, offset * euler, keys[i].Evaluate(result.percent));
                }

            var rotation       = look                                * offset;
            var invertedNormal = Quaternion.Inverse(result.rotation) * result.up;
            result.forward = rotation * Vector3.forward;
            result.up      = rotation * invertedNormal;
        }

        [Serializable]
        public class RotationKey : Key
        {
            public bool      useLookTarget;
            public Transform target;
            public Vector3   rotation = Vector3.zero;

            public RotationKey(Vector3 rotation, double f, double t, RotationModifier modifier) : base(f, t, modifier)
            {
                this.rotation = rotation;
            }
        }
    }
}