using System.Text;
using System.Text.Json;
using SocialViralAI.Models;
namespace SocialViralAI.Services
{
    public class AiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public AiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ApiKeys:OpenAI"];
        }
        public async Task<string> Analyze(List<Video> videos)
        {
            var apiKey = _apiKey;

            var data = JsonSerializer.Serialize(videos);

            var prompt = $@"
                คุณเป็น analyst ของช่องข่าว

                วิเคราะห์ข้อมูลนี้ และตอบเป็นรูปแบบ:

                📊 Insight:
                - ...

                💡 Recommendation:
                - ...

                DATA:
                {data}
                ";

            var request = new
            {
                model = "gpt-4.1-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(request);

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);
            var result = await response.Content.ReadAsStringAsync();




            var jsonDoc = JsonDocument.Parse(result);

            var content = jsonDoc
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
         
            return content;

            //return result;
        }
    }
}