using System;
using UnityEngine;

namespace MathPlus
{
    public static class MathPlus
    {
        public static int GetExponent(this float d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            return Convert.ToInt32(doubleParts[1]);
        }

        public static int GetExponent(this double d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            Debug.Log(doubleParts[1]);
            return Convert.ToInt32(doubleParts[1]);
        }

        public static float GetMantissa(this float d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            return (float) Convert.ToDouble(doubleParts[0]);
        }

        public static float GetMantissa(this double d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            return (float) Convert.ToDouble(doubleParts[0]);
        }

        private static string[] ExtractScientificNotationParts(float d)
        {
            var doubleParts = d.ToString(@"E17").Split('E');
            if (doubleParts.Length != 2)
                throw new ArgumentException();

            return doubleParts;
        }

        private static string[] ExtractScientificNotationParts(double d)
        {
            var doubleParts = d.ToString(@"E17").Split('E');
            if (doubleParts.Length != 2)
                throw new ArgumentException();

            return doubleParts;
        }
    }
}