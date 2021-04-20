using MathPlus;

namespace StaticClasses
{
    public static class StringPlus
    {
        /// <summary>
        ///     将数字转换为上标
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSuperscript(this string str)
        {
            return str.Replace("0", "\u2070")
                      .Replace("1", "\u00B9")
                      .Replace("2", "\u00B2")
                      .Replace("3", "\u00B3")
                      .Replace("4", "\u2074")
                      .Replace("5", "\u2075")
                      .Replace("6", "\u2076")
                      .Replace("7", "\u2077")
                      .Replace("8", "\u2078")
                      .Replace("9", "\u2079");
        }

        public static string ToSuperscript(this double d, int fCount)
        {
            return d.GetMantissa().ToString("f" + fCount) + "x10" + d.GetExponent().ToString().ToSuperscript();
        }
    }
}