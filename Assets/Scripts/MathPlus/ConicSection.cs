using SpacePhysic;
using UnityEngine;

namespace MathPlus
{
    public class ConicSection
    {
        private static readonly float   k = 10000;
        public                  float   a; //x^2
        public                  float   angle;
        public                  float   b;            //xy
        public                  float   c;            //y^2
        public                  float   d;            //x
        public                  float   e;            //y
        public                  float   eccentricity; //离心率
        public                  float   f;            //f
        public                  float   focalLength;
        public                  Vector2 geoCenter; //几何中心
        public                  bool    isEllipse;
        public                  float   semiMajorAxis; //长半轴
        public                  float   semiMinorAxis; //短半轴

        public ConicSection(float a, float b, float c, float d, float e, float f)
        {
            this.a = a * k;
            this.b = b * k;
            this.c = c * k;
            this.d = d * k;
            this.e = e * k;
            this.f = f * k;

            isEllipse = IsEllipse();
            geoCenter = GetGeoCenter();
            var axis = GetSemiAxis();
            semiMajorAxis = axis[0];
            semiMinorAxis = axis[1];
            focalLength   = Mathf.Sqrt(semiMajorAxis * semiMajorAxis - semiMinorAxis * semiMinorAxis);
            eccentricity  = GetEccentricity();
            angle         = GetAngle() + 90;
        }

        public ConicSection(float a, float b, float c, float theta, Vector2 geoCenter)
        {
            this.a = a * a * Mathf.Sin(theta) * Mathf.Sin(theta) + b * b * Mathf.Cos(theta) * Mathf.Cos(theta);
            this.b = 2 * (b * b - a * a) * Mathf.Sin(theta) * Mathf.Cos(theta);
            this.c = a       * a * Mathf.Cos(theta) * Mathf.Cos(theta) + b * b * Mathf.Sin(theta) * Mathf.Sin(theta);
            d      = -2      * this.a * geoCenter.x - this.b * geoCenter.y;
            e      = -this.b * geoCenter.x - 2 * this.c * geoCenter.y;
            f = this.a * geoCenter.x * geoCenter.x + this.b * geoCenter.x * geoCenter.y +
                this.c * geoCenter.y * geoCenter.y - a * a * b * b;

            isEllipse      = IsEllipse();
            this.geoCenter = geoCenter;
            semiMajorAxis  = a;
            semiMinorAxis  = b;
            focalLength    = 2 * c;
            eccentricity   = GetEccentricity();
            angle          = GetAngle(theta);
        }

        public float[] GetY(float x)
        {
            var eqA   = c;
            var eqB   = b   * x     + e;
            var eqC   = a   * x * x + d * x + f;
            var delta = eqB * eqB   - 4 * eqA * eqC;
            if (delta < 0)
                return null;
            return new[]
            {
                (-eqB - Mathf.Sqrt(delta)) / 2 * eqA,
                (-eqB + Mathf.Sqrt(delta)) / 2 * eqA
            };
        }

        public float[] GetX(float y)
        {
            var eqA   = a;
            var eqB   = b   * y     + d;
            var eqC   = c   * y * y + e * y + f;
            var delta = eqB * eqB   - 4 * eqA * eqC;
            if (delta < 0)
                return null;
            return new[]
            {
                (-eqB - Mathf.Sqrt(delta)) / 2 * eqA,
                (-eqB + Mathf.Sqrt(delta)) / 2 * eqA
            };
        }

        private bool IsEllipse()
        {
            var flag  = b * b                                   - 4         * a             * c;
            var delta = (a * c - b * b / 4) * f + b * e * d / 4 - c * d * d / 4 - a * e * e / f;
            return c * delta < 0;
        }

        private Vector2 GetGeoCenter()
        {
            return new Vector2((2 * c * d - b * e) / (b * b - 4 * a * c),
                    (2 * a * e - b * d) / (b * b - 4 * a * c)
                )
                ;
        }

        private float[] GetSemiAxis()
        {
            var axis0 = Mathf.Sqrt(
                            2 * (a * e * e + c * d * d - b * d * e + (b * b - 4 * a * c) * f) *
                            (a + c + Mathf.Sqrt(
                                (a - c) * (a - c) + b * b)
                            )
                        )
                        / (b * b - 4 * a * c);
            var axis1 = Mathf.Sqrt(
                            2 * (a * e * e + c * d * d - b * d * e + (b * b - 4 * a * c) * f) *
                            (a + c - Mathf.Sqrt(
                                (a - c) * (a - c) + b * b)
                            )
                        )
                        / (b * b - 4 * a * c);
            return new[]
            {
                Mathf.Max(Mathf.Abs(axis0), Mathf.Abs(axis1)),
                Mathf.Min(Mathf.Abs(axis0), Mathf.Abs(axis1))
            };
        }

        private float GetEccentricity()
        {
            return Mathf.Sqrt(1 - semiMinorAxis * semiMinorAxis / (semiMajorAxis * semiMajorAxis));
        }

        private float GetAngle()
        {
            if (Mathf.Abs(b) <= 0.00005f)
            {
                if (a < c)
                    return 90;
                return 0;
            }

            return Mathf.Atan(1 / b * (c - a - Mathf.Sqrt((a - c) * (a - c) + b * b))) * Mathf.Rad2Deg;
        }

        private float GetAngle(float theta)
        {
            return theta;
        }

        /// <summary>
        /// </summary>
        /// <param name="ag">角度制</param>
        /// <returns></returns>
        public Vector2 GetPolarPos(float ag)
        {
            var r = semiMinorAxis / Mathf.Sqrt(1 - eccentricity * eccentricity * Mathf.Cos(ag * Mathf.Deg2Rad) *
                Mathf.Cos(ag                                   * Mathf.Deg2Rad));
            // Debug.Log("r: " + r);
            var sumAngle = ag + angle;
            var pos = new Vector2(r * Mathf.Cos(sumAngle * Mathf.Deg2Rad),
                r * Mathf.Sin(sumAngle * Mathf.Deg2Rad));
            // Debug.Log(geoCenter + pos);
            return geoCenter + pos;
        }

        public float GetT(float m)
        {
            return 2 * Mathf.PI * Mathf.Sqrt(semiMajorAxis * semiMajorAxis * semiMajorAxis / (PhysicBase.GetG() * m));
        }

        public override string ToString()
        {
            return a + "x^2+" + b + "xy+" + c + "y^2+" + d + "x+" + e + "y+" + f + " ";
        }
    }
}