using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;

namespace Backend.Services
{
    public class AiReviewService
    {
        private readonly HttpClient _http;
        private readonly string _apiUrl = "https://api-inference.huggingface.co/models/microsoft/codebert-base";

        public AiReviewService()
        {
            // ⚙️ إعداد TLS والإهمال المؤقت للشهادات أثناء التطوير
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };

            _http = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            Console.WriteLine("Gemini Key: " + Environment.GetEnvironmentVariable("GEMINI_API_KEY"));

            // 🔑 إعداد التوكن (لو موجود)
            var token = Environment.GetEnvironmentVariable("HUGGINGFACE_TOKEN");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<AiReviewResult> AnalyzeAsync(string code)
        {
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                return new AiReviewResult { Summary = "❌ Gemini API key not found." };

            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";


            var prompt = $"You are an AI code reviewer. Analyze the following C# code and provide a short summary and improvement suggestions:\n{code}\n";

            var request = new
            {
                contents = new[]
                {
            new {
                parts = new[] {
                    new { text = prompt }
                }
            }
        }
            };

            try
            {
                var response = await _http.PostAsJsonAsync(endpoint, request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                return new AiReviewResult
                {
                    Summary = text ?? "No response from Gemini.",
                    Suggestions = ExtractSuggestions(text ?? "")
                };
            }
            catch (Exception ex)
            {
                return new AiReviewResult
                {
                    Summary = $"⚠️ Gemini request failed: {ex.Message}",
                    Suggestions = new List<Suggestion>()
                };
            }
        }

        private List<Suggestion> ExtractSuggestions(string aiText)
        {
            var suggestions = new List<Suggestion>();
            if (string.IsNullOrWhiteSpace(aiText)) return suggestions;

            var lines = aiText.Split('\n');
            int lineNumber = 1;

            foreach (var line in lines)
            {
                if (line.Contains("should", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("consider", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("recommend", StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.Add(new Suggestion
                    {
                        Line = lineNumber,
                        Message = line.Trim()
                    });
                }
                lineNumber++;
            }

            return suggestions;
        }
    }

    public class AiReviewResult
    {
        public string Summary { get; set; } = string.Empty;
        public List<Suggestion> Suggestions { get; set; } = new();
    }

    public class Suggestion
    {
        public int Line { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
