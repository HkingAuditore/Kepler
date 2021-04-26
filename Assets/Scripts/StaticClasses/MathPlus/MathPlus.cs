using System;

namespace StaticClasses.MathPlus
{
    /// <summary>
    /// 数字处理附加
    /// </summary>
    public static class MathPlus
    {
        /// <summary>
        ///     获取科学计数法指数部分
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetExponent(this float d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            return Convert.ToInt32(doubleParts[1]);
        }

        /// <summary>
        ///     获取科学计数法指数部分
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetExponent(this double d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            // Debug.Log(doubleParts[1]);
            return Convert.ToInt32(doubleParts[1]);
        }

        /// <summary>
        ///     获取科学计数法小数部分
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static float GetMantissa(this float d)
        {
            var doubleParts = ExtractScientificNotationParts(d);
            return (float) Convert.ToDouble(doubleParts[0]);
        }

        /// <summary>
        ///     获取科学计数法小数部分
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
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