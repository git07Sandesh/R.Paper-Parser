using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace R.Paper_Parser.backend
{
    public class SummaryService
    {
        private const string GeminiApiKey = "AIzaSyADtmLNh2p5N-qlqYwDVcH28opFmbD2G2k";
        
        private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        
        private readonly FileUploadService _fileUploadService;
        private readonly HttpClient _httpClient;
        
        public SummaryService(FileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
            _httpClient = new HttpClient();
        }
        
        public async Task<string> GenerateSummary(FileResult file, bool isPremium)
        {
            try
            {
                // first get text from file
                string fileContent = await _fileUploadService.ExtractTextFromFile(file);
                
                // check if extraction worked
                if (string.IsNullOrEmpty(fileContent) || 
                    fileContent.StartsWith("Error:") || 
                    fileContent.StartsWith("PDF extraction error:") ||
                    fileContent.StartsWith("DOCX extraction error:"))
                {
                    return $"Failed to extract content from the file: {fileContent}";
                }
                
                // trim big docs to avoid API limits
                const int maxApiTextLength = 30000; // 15-20 pages max
                if (fileContent.Length > maxApiTextLength)
                {
                    fileContent = fileContent.Substring(0, maxApiTextLength) + 
                        "\n\n[Content trimmed due to size limits. Summary covers first part of document.]";
                }
                
                // make sure content is meaningful
                string trimmedContent = fileContent.Trim();
                if (trimmedContent.Length < 50) // too small for real paper
                {
                    return "Text is too short or empty. Could be document protection, scanned content, or format issues. Try another document.";
                }
                
                Console.WriteLine($"Got {trimmedContent.Length} chars from {file.FileName}");
                
                // build prompt with file content
                string prompt = CreatePrompt(fileContent);
                
                // call Gemini API
                var summary = await CallGeminiApi(prompt);
                return summary;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Summary generation error: {ex}");
                return $"Error generating summary: {ex.Message}";
            }
        }
        
        private string CreatePrompt(string fileContent)
        {
            return $"Please provide a concise summary of the following research paper that highlights the key methodology and results:\n\n" +
                   "Research paper content:\n{fileContent}";
        }
        
        private async Task<string> CallGeminiApi(string prompt)
        {
            try
            {
                // make API URL with key
                string apiUrlWithKey = $"{GeminiApiUrl}?key={GeminiApiKey}";
                Console.WriteLine($"Calling Gemini API at: {GeminiApiUrl}");
                
                // create request body - format from API docs
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = prompt
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        maxOutputTokens = 2048
                    }
                };
                
                // send to Gemini API
                var response = await _httpClient.PostAsJsonAsync(apiUrlWithKey, requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                    
                    // parse response to get summary
                    try
                    {
                        var candidates = jsonResponse.GetProperty("candidates");
                        var firstCandidate = candidates[0];
                        var content = firstCandidate.GetProperty("content");
                        var parts = content.GetProperty("parts");
                        var firstPart = parts[0];
                        var text = firstPart.GetProperty("text").GetString();
                        
                        return text ?? "No text returned from API";
                    }
                    catch (Exception ex)
                    {
                        return $"Failed to parse API response: {ex.Message}";
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Gemini API Error: {response.StatusCode} - {errorResponse}");
                    
                    // try backup API if first fails
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("Trying backup API endpoint...");
                        return await TryAlternativeApiEndpoint(prompt);
                    }
                    
                    return $"API Error: {response.StatusCode} - {errorResponse}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception calling Gemini API: {ex.Message}");
                return $"API Error: {ex.Message}";
            }
        }
        
        private async Task<string> TryAlternativeApiEndpoint(string prompt)
        {
            try
            {
                // backup API with different model
                string alternativeUrl = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent";
                string apiUrlWithKey = $"{alternativeUrl}?key={GeminiApiKey}";
                
                // create request body
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = prompt
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        maxOutputTokens = 2048
                    }
                };
                
                var response = await _httpClient.PostAsJsonAsync(apiUrlWithKey, requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                    
                    // parse response to get summary
                    try
                    {
                        var candidates = jsonResponse.GetProperty("candidates");
                        var firstCandidate = candidates[0];
                        var content = firstCandidate.GetProperty("content");
                        var parts = content.GetProperty("parts");
                        var firstPart = parts[0];
                        var text = firstPart.GetProperty("text").GetString();
                        
                        return text ?? "No text returned from backup API";
                    }
                    catch (Exception ex)
                    {
                        return $"Failed to parse API response: {ex.Message}";
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return $"Backup API Error: {response.StatusCode} - {errorResponse}";
                }
            }
            catch (Exception ex)
            {
                return $"Error with backup API: {ex.Message}";
            }
        }
    }
}