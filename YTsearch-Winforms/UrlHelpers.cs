using System.Collections.Generic;
using System.Net;

namespace YTsearch_Winforms
{
    // A simple helper to replace System.Web.HttpUtility
    public static class UrlHelpers
    {
        public static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(queryString)) return dict;

            // Remove the '?' if it exists
            if (queryString.StartsWith("?")) queryString = queryString.Substring(1);

            var pairs = queryString.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    var key = WebUtility.UrlDecode(parts[0]);
                    var value = WebUtility.UrlDecode(parts[1]);
                    if (!dict.ContainsKey(key)) dict[key] = value;
                }
            }
            return dict;
        }
    }
}