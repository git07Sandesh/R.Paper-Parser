using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using dotenv.net;

namespace R.Paper_Parser.backend;
public class SummaryService
{
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api-inference.huggingface.co/models/google/pegasus-xsum";

    public SummaryService()
    {
        DotEnv.Load();

        _apiKey = Environment.GetEnvironmentVariable("HF_API_KEY");

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("Hugging Face API key not found in environment variables.");
        }
    }

    public async Task<string> GenerateSummaryAsync(string content)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var request = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] {
                new { role = "user", content = $"Summarize this research paper:\n\n{content}" }
            },
            max_tokens = 400
        };

        var json = JsonConvert.SerializeObject(request);
        var response = await client.PostAsync(_apiUrl, new StringContent(json, Encoding.UTF8, "application/json"));
        var responseContent = await response.Content.ReadAsStringAsync();

        dynamic parsed = JsonConvert.DeserializeObject(responseContent);

        if (parsed?.choices?[0]?.message?.content != null)
        {
            return parsed.choices[0].message.content;
        }
        else
        {
            return "Failed to generate summary: API returned no content.";
        }
    }
}