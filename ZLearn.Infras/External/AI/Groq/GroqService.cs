using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Common.Utils;

namespace ZLearn.Infras.External.AI.Groq
{
    public class GroqService : IAIService
    {
        private readonly ILogger<GroqService> _logger;

        public GroqService(ILogger<GroqService> logger)
        {
            _logger = logger;
        }

        public async Task<string> PromptAndAsk(string rolePrompt, string requirement)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {EnvVariableHelper.GetValue(EnvVariableNames.GROQ_API_KEY)}");
            var data = JsonSerializer.Serialize(new
            {
                model = "openai/gpt-oss-120b",
                messages = new []
                {
                    new { role = "system", content = $"{rolePrompt}" },
                    new { role = "user", content = $"{requirement}" }
                } 
            });
            var response = await client.PostAsync(
                $"https://api.groq.com/openai/v1/chat/completions",
                new StringContent(data, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new InternalErrorException($"Failed to call API: {response.StatusCode}");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine(raw);
            var resData = JsonSerializer.Deserialize<GroqResponse>(raw, options);
            return resData?.Choices.FirstOrDefault()?.Message.Content ??
                throw new InternalErrorException($"Failed to cast API response");
        }
    }
}
