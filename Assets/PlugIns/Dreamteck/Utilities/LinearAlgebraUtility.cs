using UnityEngine;

namespace Dreamteck
{
    public static class LinearAlgebraUtility
    {
        public static Vector3 ProjectOnLine(Vector3 fromPoint, Vector3 toPoint, Vector3 project)
        {
            var projectedPoint = Vector3.Project(project - fromPoint, toPoint - fromPoint) + fromPoint;
            var dir            = toPoint                                                   - fromPoint;
            var projectedDir   = projectedPoint                                            - fromPoint;
            var dot            = Vector3.Dot(projectedDir, dir);
            if (dot > 0f)
            {
                if (projectedDir.sqrMagnitude <= dir.sqrMagnitude) return projectedPoint;
                return toPoint;
            }

            return fromPoint;
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            var ab = b     - a;
            var av = value - a;
            return Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
        }

        public static float DistanceOnSphere(Vector3 from, Vector3 to, float radius)
        {
            float distance = 0;

            if (from == to)
                distance = 0;
            else if (from == -to)
                distance = Mathf.PI * radius;
            else
                distance = Mathf.Sqrt(2) * radius * Mathf.Sqrt(1.0f - Vector3.Dot(from, to));

            return distance;
        }
    }
}