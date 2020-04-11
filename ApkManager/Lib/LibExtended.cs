using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ApkManager.Lib
{
    public static class LibExtended
    {
        public static bool IsValidIPAddress(this string text)
        {
            return Regex.IsMatch(text, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(:\d{4})?$");
        }

        public static bool IsMatch(this string text, string pattern)
        {
            var match = Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
            Debug.Print("Text: {0} -> Pattern: {1} -> {2}", text, pattern, match);
            return match;
        }

        public static bool IsMatchInTextAndNotInLabel(this Tuple<string, string> tuple, string word)
        {
            var pattern = string.Format("[\\W_]{0}([\\W_]|$)", word);
            var mtext = tuple.Item1.IsMatch(pattern);
            var mlabel = tuple.Item2.IsMatch(pattern);
            return mtext && !mlabel;
        }

        public static string Append(this string text, string appendtext, bool space = true)
        {
            var separator = string.IsNullOrWhiteSpace(text) ? "" : " ";
            if (!space) separator = "";

            return string.Concat(text, separator, appendtext).Trim();
        }

        public static string AppendExtApk(this string text)
        {
            if (!text.ToLower().EndsWith(".apk"))
                return string.Concat(text, ".apk").Trim();
            else
                return text.Trim();
        }

        public static string TrimInvalidFileNameChars(this string text)
        {
            foreach (var f in text)
                foreach (var i in Path.GetInvalidFileNameChars())
                    if (f == i) text = text.Replace(f.ToString(), "");

            return text.Trim();
        }
    }
}
