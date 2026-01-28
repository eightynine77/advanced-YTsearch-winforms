using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YTsearch_Winforms
{
    public class YouTubeSearchService
    {
        private readonly HttpClient _httpClient;

        // [IMPORTANT] REPLACE THIS WITH YOUR ACTUAL VERCEL DOMAIN
        private const string VercelApiUrl = "https://advanced-youtube-search.vercel.app/api/search";

        public YouTubeSearchService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> SearchAsync(string query, string frontendKey, string pageToken, string publishedAfter, string publishedBefore)
        {
            // 1. Determine which API Key/Method to use
            // Priority: Key sent from JS > Key saved in Encrypted File > Use Vercel Proxy
            string activeKey = frontendKey;

            if (string.IsNullOrWhiteSpace(activeKey) || activeKey == "undefined")
            {
                activeKey = SecureSettings.LoadApiKey();
            }

            // 2. MODE SWITCHING
            if (!string.IsNullOrWhiteSpace(activeKey))
            {
                // === MODE 1: DIRECT LOCAL SEARCH (User provided a key) ===
                // We must fetch from Google AND filter locally (simulating search.js logic)
                return await PerformLocalSearch(query, activeKey, pageToken, publishedAfter, publishedBefore);
            }
            else
            {
                // === MODE 2: VERCEL PROXY (No key provided) ===
                // We ask your Vercel site to do the work. It has the secret key hidden.
                return await PerformVercelProxySearch(query, pageToken, publishedAfter, publishedBefore);
            }
        }

        private async Task<string> PerformVercelProxySearch(string query, string pageToken, string publishedAfter, string publishedBefore)
        {
            try
            {
                // Construct URL to your Vercel site
                var url = $"{VercelApiUrl}?q={Uri.EscapeDataString(query)}";
                if (!string.IsNullOrEmpty(pageToken) && pageToken != "undefined") url += $"&pageToken={pageToken}";
                if (!string.IsNullOrEmpty(publishedAfter) && publishedAfter != "undefined") url += $"&publishedAfter={publishedAfter}";
                if (!string.IsNullOrEmpty(publishedBefore) && publishedBefore != "undefined") url += $"&publishedBefore={publishedBefore}";

                // Just fetch and return exactly what Vercel sends back
                return await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = new { message = $"Vercel Connection Error: {ex.Message}" } });
            }
        }

        private async Task<string> PerformLocalSearch(string query, string apiKey, string pageToken, string publishedAfter, string publishedBefore)
        {
            try
            {
                var baseUrl = "https://www.googleapis.com/youtube/v3/search";
                var url = $"{baseUrl}?part=snippet&type=video&maxResults=50&key={apiKey}&q={Uri.EscapeDataString(query)}";

                if (!string.IsNullOrEmpty(pageToken) && pageToken != "undefined") url += $"&pageToken={pageToken}";
                if (!string.IsNullOrEmpty(publishedAfter) && publishedAfter != "undefined") url += $"&publishedAfter={publishedAfter}";
                if (!string.IsNullOrEmpty(publishedBefore) && publishedBefore != "undefined") url += $"&publishedBefore={publishedBefore}";

                var response = await _httpClient.GetAsync(url);
                var jsonString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return jsonString;

                // --- LOCAL FILTERING LOGIC (Replicating search.js) ---
                JsonNode root = JsonNode.Parse(jsonString);
                if (root["items"] is JsonArray items)
                {
                    var searchTerms = query.Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var filteredItems = items.Where(item =>
                    {
                        string title = item["snippet"]?["title"]?.ToString() ?? "";
                        string description = item["snippet"]?["description"]?.ToString() ?? "";
                        string contentToCheck = (title + " " + description).ToLower();

                        return searchTerms.All(term =>
                        {
                            string escapedTerm = Regex.Escape(term);
                            return Regex.IsMatch(contentToCheck, $@"\b{escapedTerm}\b");
                        });
                    }).ToArray();

                    root["items"] = new JsonArray(filteredItems);
                }
                return root.ToString();
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = new { message = $"Local Search Error: {ex.Message}" } });
            }
        }
    }
}