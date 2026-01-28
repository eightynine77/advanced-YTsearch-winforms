using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes; // Requires System.Text.Json
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class YouTubeSearchService
{
    // PASTE YOUR FALLBACK API KEY HERE
    // This acts like process.env.YOUTUBE_API_KEY
    private readonly string _defaultApiKey = "";
    private readonly HttpClient _httpClient;

    public YouTubeSearchService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> SearchAsync(string query, string userApiKey, string pageToken, string publishedAfter, string publishedBefore)
    {
        // 1. Determine which API Key to use (User's key > Default key)
        string apiKey = !string.IsNullOrWhiteSpace(userApiKey) && userApiKey != "undefined"
            ? userApiKey
            : _defaultApiKey;

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_DEFAULT_API_KEY_HERE")
        {
            return JsonSerializer.Serialize(new { error = new { message = "API Key is missing. Please configure it in settings." } });
        }

        // 2. Build the YouTube API URL
        // We use Uri.EscapeDataString which is the C# equivalent of encodeURIComponent
        string url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&maxResults=50&key={apiKey}&q={Uri.EscapeDataString(query)}";

        if (!string.IsNullOrEmpty(pageToken) && pageToken != "undefined") url += $"&pageToken={pageToken}";
        if (!string.IsNullOrEmpty(publishedAfter) && publishedAfter != "undefined") url += $"&publishedAfter={publishedAfter}";
        if (!string.IsNullOrEmpty(publishedBefore) && publishedBefore != "undefined") url += $"&publishedBefore={publishedBefore}";

        try
        {
            // 3. Fetch from YouTube (The "Fetch" part of search.js)
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            string jsonString = await response.Content.ReadAsStringAsync();

            // Parse JSON dynamically so we don't need to create huge C# classes
            JsonNode root = JsonNode.Parse(jsonString);

            if (!response.IsSuccessStatusCode)
            {
                // Pass the error error back to the frontend
                return jsonString;
            }

            // 4. The Logic: "Match Words" Filter (The "Filter" part of search.js)
            // We look at root["items"] and filter it just like your JS filter() function
            if (root["items"] is JsonArray items)
            {
                var searchTerms = query.Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Filter the items
                var filteredItems = items.Where(item =>
                {
                    string title = item["snippet"]?["title"]?.ToString() ?? "";
                    string description = item["snippet"]?["description"]?.ToString() ?? "";
                    string contentToCheck = (title + " " + description).ToLower();

                    // Check if ALL terms match using Regex (Whole Word Match)
                    return searchTerms.All(term =>
                    {
                        // Escaping regex characters, just like in your JS
                        string escapedTerm = Regex.Escape(term);
                        // \b matches word boundaries
                        return Regex.IsMatch(contentToCheck, $@"\b{escapedTerm}\b");
                    });
                }).ToArray();

                // Replace the original items with the filtered list
                root["items"] = new JsonArray(filteredItems);
            }

            // 5. Return the clean, filtered JSON string
            return root.ToString();
        }
        catch (Exception ex)
        {
            // Handle network crashes or parsing errors
            return JsonSerializer.Serialize(new { error = new { message = ex.Message } });
        }
    }
}