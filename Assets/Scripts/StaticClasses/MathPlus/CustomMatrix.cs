﻿using MathNet.Numerics.LinearAlgebra;

namespace StaticClasses.MathPlus
{
    /// <summary>
    /// 矩阵
    /// </summary>
    public static class CustomMatrix
    {
        public static Vector<float> SolveZeroEquations(Matrix<float> matrixA)
        {
            var b = Vector<float>.Build.Dense(new float[] {0, 0, 0, 0, 0, 0});
            var x = matrixA.Solve(b);
            return x;
        }
    }
}