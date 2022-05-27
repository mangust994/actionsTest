using System;

namespace HHAzureImageStorage.BL.Extensions
{
    public static class StringExtension
    {
        public static string TruncateLongString(this string str, int maxLength) => string.IsNullOrEmpty(str) ? str : str.Substring(0, Math.Min(str.Length, maxLength));

        public static string Default(this string _this)
        {
            return string.IsNullOrEmpty(_this) ? string.Empty : _this;
        }

        public static bool Is(this string left, string value)
        {
            return left.Is(value, true);
        }

        public static bool Is(this string left, string value, bool ignoreCase)
        {
            if (left == null || value == null)
            {
                return false;
            }
            var ic = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return left.Length == value.Length && left.IndexOf(value, ic) == 0;
        }
    }
}
