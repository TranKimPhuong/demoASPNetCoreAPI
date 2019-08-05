using System.Text.RegularExpressions;

namespace System.Collections.Generic
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns items in the collection that match the specified search pattern.
        /// <para>Similar to https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx </para>
        /// </summary>
        /// <param name="searchPattern">The search string to match against collection item.
        ///  This parameter can contain wildcard (* and ?) characters.</param>
        public static IEnumerable<string> Search(this IEnumerable<string> source, string searchPattern)
        {
            foreach (var item in source)
            {
                if (item.IsMatch(searchPattern))
                    yield return item;
            }
        }
        static bool IsMatch(this string input, string pattern)
        {
            var regx = "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            return new Regex(regx, RegexOptions.IgnoreCase | RegexOptions.Singleline)
                    .IsMatch(input);
        }
    }
}
