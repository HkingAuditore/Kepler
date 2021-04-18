using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    public class Ngon : SplinePrimitive
    {
        public float radius = 1f;
        public int   sides  = 3;

        public override Spline.Type GetSplineType()
        {
            return Spline.Type.Linear;
        }

        protected override void Generate()
        {
            base.Generate();
            closed = true;
            CreatePoints(sides + 1, SplinePoint.Type.SmoothMirrored);
            for (var i = 0; i < sides; i++)
            {
                var percent = (float) i                                             / sides;
                var pos     = Quaternion.AngleAxis(360f * percent, Vector3.forward) * Vector3.up * radius;
                points[i].SetPosition(pos);
            }

            points[points.Length - 1] = points[0];
        }
    }
}