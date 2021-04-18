using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    public class Star : SplinePrimitive
    {
        public float depth  = 0.5f;
        public float radius = 1f;
        public int   sides  = 5;

        public override Spline.Type GetSplineType()
        {
            return Spline.Type.Linear;
        }

        protected override void Generate()
        {
            base.Generate();
            closed = true;
            CreatePoints(sides * 2 + 1, SplinePoint.Type.SmoothMirrored);
            var innerRadius = radius * depth;
            for (var i = 0; i < sides * 2; i++)
            {
                var percent = i / (float) (sides * 2);
                var pos = Quaternion.AngleAxis(180 + 360f * percent, Vector3.forward) * Vector3.up *
                          ((float) i % 2f == 0 ? radius : innerRadius);
                points[i].SetPosition(pos);
            }

            points[points.Length - 1] = points[0];
        }
    }
}