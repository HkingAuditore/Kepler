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
        public float a; //x^2
        public float b; //xy
        public float c; //y^2
        public float d; //x
        public float e; //y
        public float f; //f
        public bool isEllipse;
        public Vector2 geoCenter; //几何中心
        public float semiMajorAxis; //长半轴
        public float semiMinorAxis; //短半轴
        public float eccentricity; //离心率
        public float angle;
        public float focalLength;

        public ConicSection(float a, float b, float c, float d, float e, float f)
        {
            this.a = a * 10000;
            this.b = b * 10000;
            this.c = c * 10000;
            this.d = d * 10000;
            this.e = e * 10000;
            this.f = f * 10000;

            isEllipse = IsEllipse();
            geoCenter = GetGeoCenter();
            var axis = GetSemiAxis();
            semiMajorAxis = axis[0];
            semiMinorAxis = axis[1];
            focalLength = Mathf.Sqrt(semiMajorAxis * semiMajorAxis - semiMinorAxis * semiMinorAxis);
            eccentricity = GetEccentricity();
            angle = GetAngle() + 90;
        }

        public float[] GetY(float x)
        {
            var eqA = c;
            var eqB = b * x + e;
            var eqC = a * x * x + d * x + f;
            var delta = eqB * eqB - 4 * eqA * eqC;
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
            var eqA = a;
            var eqB = b * y + d;
            var eqC = c * y * y + e * y + f;
            var delta = eqB * eqB - 4 * eqA * eqC;
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
            var flag = b * b - 4 * a * c;
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
            return Mathf.Sqrt(1 - ((semiMinorAxis * semiMinorAxis) / (semiMajorAxis * semiMajorAxis)));
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

        /// <summary>
        /// </summary>
        /// <param name="ag">角度制</param>
        /// <returns></returns>
        public Vector2 GetPolarPos(float ag)
        {
            var r = semiMinorAxis / Mathf.Sqrt(1 - eccentricity * eccentricity * Mathf.Cos(ag * Mathf.Deg2Rad) *
                                               Mathf.Cos(ag * Mathf.Deg2Rad));
            // Debug.Log("r: " + r);
            var sumAngle = ag + angle;
            var pos = new Vector2(r * Mathf.Cos(sumAngle * Mathf.Deg2Rad),
                                  r * Mathf.Sin(sumAngle * Mathf.Deg2Rad));
            // Debug.Log(geoCenter + pos);
            return geoCenter + pos;
        }

        public float GetT(float m)
        {
            return 2 * Mathf.PI * Mathf.Sqrt((semiMajorAxis * semiMajorAxis * semiMajorAxis) / (PhysicBase.GetG() * m));
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

            var s = d.Transpose() * d;
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
            var a = svd.U.Column(0);

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
    }
}