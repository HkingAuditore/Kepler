using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using SpacePhysic;
using UnityEngine;

namespace MathPlus
{
    public delegate float ConicSectionDelegate(float x);

    public static class CustomMatrix
    {
        public static Vector<float> SolveZeroEquations(Matrix<float> matrixA)
        {
            var b = Vector<float>.Build.Dense(new float[] {0, 0, 0, 0, 0, 0});
            var x = matrixA.Solve(b);
            return x;
        }
    }

    public class ConicSection
    {
        private static float   k = 10000;
        public         float   a; //x^2
        public         float   angle;
        public         float   b;            //xy
        public         float   c;            //y^2
        public         float   d;            //x
        public         float   e;            //y
        public         float   eccentricity; //离心率
        public         float   f;            //f
        public         float   focalLength;
        public         Vector2 geoCenter; //几何中心
        public         bool    isEllipse;
        public         float   semiMajorAxis; //长半轴
        public         float   semiMinorAxis; //短半轴

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
        public ConicSection(float a, float b, float c,float theta,Vector2 geoCenter)
        {
            this.a =  (a * a *Mathf.Sin(theta) *Mathf.Sin(theta) + b * b *Mathf.Cos(theta) *Mathf.Cos(theta));
            this.b =  (2 * (b * b - a * a) * Mathf.Sin(theta) * Mathf.Cos(theta));
            this.c =  (a  * a     * Mathf.Cos(theta) * Mathf.Cos(theta) + b * b * Mathf.Sin(theta) * Mathf.Sin(theta));
            this.d =  (-2 *this.a *geoCenter.x                          - this.b*geoCenter.y);
            this.e =  (-this.b *geoCenter.x - 2*this.c*geoCenter.y);
            this.f =  (this.a*geoCenter.x*geoCenter.x + this.b*geoCenter.x*geoCenter.y+this.c*geoCenter.y*geoCenter.y-a*a*b*b);

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

    public static class CustomSolver
    {
        private static float[] ConvertPointToEquation(Vector2 point)
        {
            return new[]
                   {
                       point.x * point.x,
                       point.x * point.y,
                       point.y * point.y,
                       point.x,
                       point.y,
                       1f
                   };
        }


        public static ConicSection SolveConicSection(Vector2 point0, Vector2 point1, Vector2 point2, Vector2 point3,
                                                     Vector2 point4, Vector2 point5)
        {
            var a = Matrix<float>.Build.DenseOfRowArrays(
                                                         ConvertPointToEquation(point0),
                                                         ConvertPointToEquation(point1),
                                                         ConvertPointToEquation(point2),
                                                         ConvertPointToEquation(point3),
                                                         ConvertPointToEquation(point4),
                                                         ConvertPointToEquation(point5)
                                                        );
            var result = CustomMatrix.SolveZeroEquations(a);
            // Debug.Log("result: " +result);

            return new ConicSection(result[0],
                                    result[1],
                                    result[2],
                                    result[3],
                                    result[4],
                                    result[5]);
        }

        public static ConicSection FitConicSection(List<Vector2> points)
        {
            var d = Matrix<float>.Build.Dense(points.Count, 6);
            for (var i = 0; i < points.Count; i++) d.SetRow(i, ConvertPointToEquation(points[i]));


            var d1 = d.SubMatrix(0, points.Count, 0, 3);
            var d2 = d.SubMatrix(0, points.Count, 3, 3);

            // Debug.Log(d1);
            // Debug.Log(d2);

            var s  = d.Transpose()  * d;
            var s1 = d1.Transpose() * d1;
            var s2 = d1.Transpose() * d2;
            var s3 = d2.Transpose() * d2;

            var c = Matrix<float>.Build.DenseOfArray(new float[,]
                                                     {
                                                         {0, 0, 2, 0, 0, 0},
                                                         {0, -1, 0, 0, 0, 0},
                                                         {2, 0, 0, 0, 0, 0},
                                                         {0, 0, 0, 0, 0, 0},
                                                         {0, 0, 0, 0, 0, 0},
                                                         {0, 0, 0, 0, 0, 0}
                                                     });

            var sc = s.Inverse() * c;
            var c1 = Matrix<float>.Build.DenseOfArray(new float[,]
                                                      {
                                                          {0, 0, 2},
                                                          {0, -1, 0},
                                                          {2, 0, 0}
                                                      });

            // var m = c1.Inverse() * (s1 - s2 * s3.Inverse() * s2.Transpose());

            var svd = sc.Svd();
            var a   = svd.U.Column(0);

            // Evd<float> eigen = m.Evd();
            // var eigenVectors = eigen.EigenVectors;
            // Vector<float> a1 = Vector<float>.Build.Dense(6);
            // float tmp = 9999f;
            // for (int i = 0; i < eigenVectors.ColumnCount; i++)
            // {
            //     Debug.Log(eigenVectors.Column(i));
            //     float t = Mathf.Abs((eigenVectors.Column(i).ToRowMatrix() * c1 * eigenVectors.Column(i))[0]);
            //     if (t < tmp)
            //     {
            //         tmp = t;
            //         a1 = eigenVectors.Column(i);
            //         
            //     }
            //     // Debug.Log(eigenVectors.Column(i).ToRowMatrix()*c1*eigenVectors.Column(i));
            // }
            //
            // var a2 = s3.Inverse() * s2.Transpose() * a1;
            // return new ConicSection(a1[0],
            //                         a1[1],
            //                         a1[2],
            //                         a2[0],
            //                         a2[1],
            //                         a2[2]);
            return new ConicSection(
                                    a[0],
                                    a[1],
                                    a[2],
                                    a[3],
                                    a[4],
                                    a[5]
                                   );
        }

        public static Vector3 GetCircleOrbitVelocity(Vector3 targetPos, Vector3 centerPos,float centerMass)
        {
            float   r     = Vector3.Distance(targetPos, centerPos);
            float   speed = Mathf.Sqrt(PhysicBase.GetG() * centerMass / r);
            Vector3 dir   = (Quaternion.AngleAxis(90, Vector3.up) * (centerPos - targetPos)).normalized;
            return dir * speed;
        }

        public static ConicSection CalculateOrbit(Vector2 targetPos,  Vector2 oriPos, Vector2 targetVelocity,
                                                  float   targetMass, float   oriMass)
        {
            float miu = PhysicBase.GetG() * oriMass;

            float h = targetPos.x * targetVelocity.y - targetPos.y * targetVelocity.x;
            float r = Mathf.Sqrt(targetPos.x * targetPos.x + targetPos.y * targetPos.y);
            float a = (miu * r) /
                      (2 * miu - r * (targetVelocity.x * targetVelocity.x + targetVelocity.y * targetVelocity.y));
            Vector2 ev = new Vector2(targetPos.x / r - (h * targetVelocity.y) / miu,
                                     targetPos.y / r + (h * targetVelocity.x)  / miu);
            Vector2 f2           = 2             * a * ev;
            Vector2 geoCenter    = (f2 + oriPos) / 2;
            float   e            = ev.magnitude;
            float   c            = e * a;
            float   b            = Mathf.Sqrt(a * a - c * c);
            Vector2 orbitHorizon = (f2 - oriPos).normalized;
            float   theta = 0;
            if (orbitHorizon.y > 0)
            {
                theta = Vector2.Angle(new Vector2(1, 0), orbitHorizon);
            }
            else
            {
                theta =180 - Vector2.Angle(new Vector2(1, 0), orbitHorizon);

            }
            // Debug.Log("a = " + a);
            // Debug.Log("b = " + b);
            // Debug.Log("e = " + e);
            // Debug.Log("ev = " + ev);
            // Debug.Log("geoCenter = " + geoCenter);
            // Debug.Log("theta= " + theta);
            return new ConicSection(a, b, c,theta, geoCenter);
        }
    }

    public static class MathPlus
    {
        public static int GetExponent(this float d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            return Convert.ToInt32(doubleParts[1]);
        }

        public static float GetMantissa(this float d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            return (float)Convert.ToDouble(doubleParts[0]);
        }

        private static string[] ExtractScientificNotationParts(float d)
        {
            var doubleParts = d.ToString(@"E17").Split('E');
            if (doubleParts.Length != 2)
                throw new ArgumentException();

            return doubleParts;
        }
    }
}