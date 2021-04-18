using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    public class Line : SplinePrimitive
    {
        public float length   = 1f;
        public bool  mirror   = true;
        public int   segments = 1;

        public override Spline.Type GetSplineType()
        {
            return Spline.Type.Linear;
        }

        protected override void Generate()
        {
            base.Generate();
            closed = false;
            CreatePoints(segments + 1, SplinePoint.Type.SmoothMirrored);
            var origin         = Vector3.zero;
            if (mirror) origin = -Vector3.up * length * 0.5f;
            for (var i = 0; i < points.Length; i++)
                points[i].position = origin + Vector3.up * length * ((float) i / (points.Length - 1));
        }
    }
}