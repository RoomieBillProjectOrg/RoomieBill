using System.Text;
using System.Text.Json;

namespace Roomiebill.Server.Common
{
    /// <summary>
    /// Provides integration with Google's Gemini AI model for generating text responses.
    /// </summary>
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        /// <summary>
        /// Initializes a new instance of the GeminiService.
        /// </summary>
        /// <param name="config">Configuration containing Gemini API settings.</param>
        public GeminiService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = "AIzaSyD1hVRYGznSpVXOL4qR5YxXXA-btLnGBuI"; // Set this in appsettings.json
        }

        /// <summary>
        /// Generates a response from Gemini AI based on the provided prompt.
        /// </summary>
        /// <param name="prompt">The text prompt to send to Gemini.</param>
        /// <returns>The generated response text from Gemini.</returns>
        /// <exception cref="ArgumentNullException">When prompt is null or empty.</exception>
        /// <exception cref="Exception">When the Gemini API call fails.</exception>
        public async Task<string> GetFeedbackFromGeminiAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentNullException(nameof(prompt), "Prompt cannot be empty.");

            var url = $"{API_URL}?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API failed: {responseString}");

            using var doc = JsonDocument.Parse(responseString);
            var output = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return output ?? string.Empty;
        }

        public async Task<string> ExtractDataFromTextWithGeminiAsync(string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API failed: {responseString}");

            using var doc = JsonDocument.Parse(responseString);
            var output = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return output;
        }
    }
}
