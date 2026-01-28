using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YTsearch_Winforms
{
    public partial class YouTubeSearchService : Component
    {
        //private readonly HttpClient _httpClient;

        public YouTubeSearchService()
        {
            InitializeComponent();
            //_httpClient = new HttpClient();
        }

        //public YouTubeSearchService(IContainer container)
        //{
        //    container.Add(this);
        //    InitializeComponent();
        //    _httpClient = new HttpClient();
        //}

        //// The Main Function: Replaces 'export default async function handler(req, res)'
        //public async Task<string> SearchAsync(string query, string apiKey, string pageToken, string publishedAfter, string publishedBefore)
        //{
        //    // 1. Validation (Matches 'if (!q)...')
        //    if (string.IsNullOrWhiteSpace(query))
        //    {
        //        return JsonSerializer.Serialize(new { error = "Search query 'q' is required." });
        //    }

        //    // 2. Build URL (Matches 'let url = ...')
        //    var baseUrl = "https://www.googleapis.com/youtube/v3/search";
        //    var url = $"{baseUrl}?part=snippet&type=video&maxResults=50&key={apiKey}&q={Uri.EscapeDataString(query)}";

        //    if (!string.IsNullOrEmpty(pageToken)) url += $"&pageToken={pageToken}";
        //    if (!string.IsNullOrEmpty(publishedAfter)) url += $"&publishedAfter={publishedAfter}";
        //    if (!string.IsNullOrEmpty(publishedBefore)) url += $"&publishedBefore={publishedBefore}";

        //    try
        //    {
        //        // 3. Fetch Data (Matches 'const youtubeResponse = await fetch(url)')
        //        var response = await _httpClient.GetAsync(url);
        //        var jsonString = await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            // Pass the error error straight back to UI
        //            return jsonString;
        //        }

        //        // 4. Parse & Filter (Matches 'if (data.items && q)...')
        //        // We use JsonNode so we can modify the array without creating a C# class for the whole YouTube API
        //        JsonNode rootNode = JsonNode.Parse(jsonString);
        //        JsonArray items = rootNode["items"]?.AsArray();

        //        if (items != null && items.Count > 0)
        //        {
        //            // Split query into terms (Matches 'q.trim().toLowerCase().split...')
        //            var searchTerms = query.Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //            // Filter logic: We iterate backwards because we are removing items
        //            for (int i = items.Count - 1; i >= 0; i--)
        //            {
        //                var item = items[i];
        //                string title = item["snippet"]?["title"]?.ToString() ?? "";
        //                string description = item["snippet"]?["description"]?.ToString() ?? "";

        //                // Combine title + desc (Matches 'const contentToCheck = ...')
        //                string contentToCheck = (title + " " + description).ToLower();

        //                // Check all terms (Matches 'return searchTerms.every...')
        //                bool allTermsMatch = searchTerms.All(term =>
        //                {
        //                    // Create Regex for whole word (Matches 'new RegExp(`\\b${escapedTerm}\\b`)')
        //                    string escapedTerm = Regex.Escape(term);
        //                    return Regex.IsMatch(contentToCheck, $@"\b{escapedTerm}\b");
        //                });

        //                // If NOT a match, remove it from the array
        //                if (!allTermsMatch)
        //                {
        //                    items.RemoveAt(i);
        //                }
        //            }
        //        }

        //        // 5. Return the modified JSON (Matches 'res.status(200).json(data)')
        //        return rootNode.ToJsonString();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Error handling
        //        return JsonSerializer.Serialize(new { error = $"Internal Error: {ex.Message}" });
        //    }
        //}
    }
}