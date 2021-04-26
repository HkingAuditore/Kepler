using System.Text;
using StaticClasses.MathPlus;

namespace StaticClasses
{
    /// <summary>
    /// 字符串附加
    /// </summary>
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
        
        public static string ToSuperscript(this double d, int fCount,int plusExponent)
        {
            return d.GetMantissa().ToString("f" + fCount) + "x10" + (d.GetExponent() + plusExponent).ToString().ToSuperscript();
        }
        public static string ToSuperscript(this float d, int fCount,int plusExponent)
        {
            return d.GetMantissa().ToString("f" + fCount) + "x10" + (d.GetExponent() + plusExponent).ToString().ToSuperscript();
        }

        public static string RichTextFilter(this string text)
        {
            StringBuilder str          = new StringBuilder();
            bool          isInRichText = false;
            foreach (var t in text)
            {
                if (t == '<' && !isInRichText) isInRichText = true;
                if (t == '>' && isInRichText) isInRichText  = false;
                str.Append(isInRichText ? "" : t.ToString());
            }

            return str.ToString();
        }
    }
}