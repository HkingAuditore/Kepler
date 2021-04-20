using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using SpacePhysic;
using UnityEngine;

namespace MathPlus
{
    public static class CustomSolver
    {
        /// <summary>
        ///     将点转换为方程
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     求6点拟合椭圆
        /// </summary>
        /// <param name="point0"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <param name="point4"></param>
        /// <param name="point5"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     求多点拟合椭圆
        /// </summary>
        /// <param name="points">点集</param>
        /// <returns></returns>
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

        /// <summary>
        ///     求圆形环绕轨道速度
        /// </summary>
        /// <param name="targetPos">绕转星体位置</param>
        /// <param name="centerPos">中心星体位置</param>
        /// <param name="centerMass">中心天体质量</param>
        /// <returns></returns>
        public static Vector3 GetCircleOrbitVelocity(Vector3 targetPos, Vector3 centerPos, float centerMass)
        {
            var r     = Vector3.Distance(targetPos, centerPos);
            var speed = Mathf.Sqrt(PhysicBase.GetG() * centerMass / r);
            var dir   = (Quaternion.AngleAxis(90, Vector3.up) * (centerPos - targetPos)).normalized;
            return dir * speed;
        }

        /// <summary>
        ///     根据开普勒三定律求解轨道
        /// </summary>
        /// <param name="targetPos">绕转星体位置</param>
        /// <param name="oriPos">中心星体位置</param>
        /// <param name="targetVelocity">绕转星体速度</param>
        /// <param name="targetMass">绕转星体质量</param>
        /// <param name="oriMass">中心天体质量</param>
        /// <returns></returns>
        public static ConicSection CalculateOrbit(Vector2 targetPos,  Vector2 oriPos, Vector2 targetVelocity,
                                                  float   targetMass, float   oriMass)
        {
            var miu = PhysicBase.GetG() * oriMass;

            var h = targetPos.x * targetVelocity.y - targetPos.y * targetVelocity.x;
            var r = Mathf.Sqrt(targetPos.x * targetPos.x + targetPos.y * targetPos.y);
            var a = miu * r /
                    (2 * miu - r * (targetVelocity.x * targetVelocity.x + targetVelocity.y * targetVelocity.y));
            var ev = new Vector2(targetPos.x / r - h * targetVelocity.y / miu,
                                 targetPos.y / r + h * targetVelocity.x / miu);
            var   f2           = 2             * a * ev;
            var   geoCenter    = (f2 + oriPos) / 2;
            var   e            = ev.magnitude;
            var   c            = e * a;
            var   b            = Mathf.Sqrt(a * a - c * c);
            var   orbitHorizon = (f2 - oriPos).normalized;
            float theta        = 0;
            if (orbitHorizon.y > 0)
                theta = Vector2.Angle(new Vector2(1, 0), orbitHorizon);
            else
                theta = 180 - Vector2.Angle(new Vector2(1, 0), orbitHorizon);
            // Debug.Log("a = " + a);
            // Debug.Log("b = " + b);
            // Debug.Log("e = " + e);
            // Debug.Log("ev = " + ev);
            // Debug.Log("geoCenter = " + geoCenter);
            // Debug.Log("theta= " + theta);
            return new ConicSection(a, b, c, theta, geoCenter);
        }
    }
}