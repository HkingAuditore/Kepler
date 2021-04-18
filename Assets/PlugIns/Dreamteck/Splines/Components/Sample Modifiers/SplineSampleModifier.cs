using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
    [Serializable]
    public class SplineSampleModifier
    {
        public float blend = 1f;

        public virtual List<Key> GetKeys()
        {
            return new List<Key>();
        }

        public virtual void SetKeys(List<Key> input)
        {
            for (var i = 0; i < input.Count; i++) input[i].modifier = this;
        }

        public virtual void Apply(SplineSample result)
        {
        }

        public virtual void Apply(SplineSample source, SplineSample destination)
        {
            destination.CopyFrom(source);
            Apply(destination);
        }

        [Serializable]
        public class Key
        {
            [SerializeField] private double _featherStart, _featherEnd, _centerStart = 0.25, _centerEnd = 0.75;

            [SerializeField] internal SplineSampleModifier modifier;

            public AnimationCurve interpolation;
            public float          blend = 1f;

            internal Key(double f, double t, SplineSampleModifier modifier)
            {
                this.modifier = modifier;
                start         = f;
                end           = t;
                interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            }

            public double start
            {
                get => _featherStart;
                set
                {
                    if (value != _featherStart) _featherStart = DMath.Clamp01(value);
                }
            }

            public double end
            {
                get => _featherEnd;
                set
                {
                    if (value != _featherEnd) _featherEnd = DMath.Clamp01(value);
                }
            }

            public double centerStart
            {
                get => _centerStart;
                set
                {
                    if (value != _centerStart)
                    {
                        _centerStart = DMath.Clamp01(value);
                        if (_centerStart > _centerEnd) _centerStart = _centerEnd;
                    }
                }
            }

            public double centerEnd
            {
                get => _centerEnd;
                set
                {
                    if (value != _centerEnd)
                    {
                        _centerEnd = DMath.Clamp01(value);
                        if (_centerEnd < _centerStart) _centerEnd = _centerStart;
                    }
                }
            }


            public double globalCenterStart
            {
                get => LocalToGlobalPercent(centerStart);
                set => centerStart = DMath.Clamp01(GlobalToLocalPercent(value));
            }

            public double globalCenterEnd
            {
                get => LocalToGlobalPercent(centerEnd);
                set => centerEnd = DMath.Clamp01(GlobalToLocalPercent(value));
            }

            public double position
            {
                get
                {
                    var center = DMath.Lerp(_centerStart, _centerEnd, 0.5);
                    if (start > end)
                    {
                        var pos               = DMath.Lerp(_featherStart, _featherEnd, center);
                        var fromToEndDistance = 1.0 - _featherStart;
                        var centerDistance    = center * (fromToEndDistance + _featherEnd);
                        pos = _featherStart + centerDistance;
                        if (pos > 1.0) pos -= 1.0;
                        return pos;
                    }

                    return DMath.Lerp(_featherStart, _featherEnd, center);
                }
                set
                {
                    var delta = value - position;
                    start += delta;
                    end   += delta;
                }
            }

            private double GlobalToLocalPercent(double t)
            {
                if (_featherStart > _featherEnd)
                {
                    if (t > _featherStart)
                        return DMath.InverseLerp(_featherStart, _featherStart + (1.0 - _featherStart) + _featherEnd, t);
                    if (t < _featherEnd) return DMath.InverseLerp(-(1.0 - _featherStart), _featherEnd, t);
                    return 0f;
                }

                return DMath.InverseLerp(_featherStart, _featherEnd, t);
            }

            private double LocalToGlobalPercent(double t)
            {
                if (_featherStart > _featherEnd)
                {
                    t = DMath.Lerp(_featherStart, _featherStart + (1.0 - _featherStart) + _featherEnd, t);
                    if (t > 1.0) t -= 1.0;
                    return t;
                }

                return DMath.Lerp(_featherStart, _featherEnd, t);
            }

            public float Evaluate(double t)
            {
                t = (float) GlobalToLocalPercent(t);
                if (t < _centerStart) return interpolation.Evaluate((float) t / (float) _centerStart) * blend;

                if (t > _centerEnd)
                    return interpolation.Evaluate(1f - (float) DMath.InverseLerp(_centerEnd, 1.0, t)) * blend;
                return interpolation.Evaluate(1f) * blend;
            }

            public virtual Key Duplicate()
            {
                var newKey = new Key(start, end, modifier);
                newKey._centerStart  = _centerStart;
                newKey._centerEnd    = _centerEnd;
                newKey.blend         = blend;
                newKey.interpolation = DuplicateUtility.DuplicateCurve(interpolation);
                return newKey;
            }
        }
    }
}