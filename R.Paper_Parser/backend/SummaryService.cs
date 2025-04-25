using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace R.Paper_Parser.backend
{
    public class SummaryResult
    {
        public string ExtractedText { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

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
        
        public async Task<SummaryResult> GenerateSummary(FileResult file, bool isPremium)
        {
            var result = new SummaryResult();
            
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
                    result.ErrorMessage = $"Failed to extract content from the file: {fileContent}";
                    result.IsSuccess = false;
                    return result;
                }
                
                // Store the extracted text in the result
                result.ExtractedText = fileContent;
                
                // trim big docs to avoid API limits
                string trimmedContent = fileContent;
                const int maxApiTextLength = 30000; // 15-20 pages max
                if (trimmedContent.Length > maxApiTextLength)
                {
                    trimmedContent = trimmedContent.Substring(0, maxApiTextLength) + 
                        "\n\n[Content trimmed due to size limits. Summary covers first part of document.]";
                }
                
                // make sure content is meaningful
                if (trimmedContent.Trim().Length < 50) // too small for real paper
                {
                    result.ErrorMessage = "Text is too short or empty. Could be document protection, scanned content, or format issues. Try another document.";
                    result.IsSuccess = false;
                    return result;
                }
                
                Console.WriteLine($"Got {trimmedContent.Length} chars from {file.FileName}");
                
                // build prompt with file content
                string prompt = CreatePrompt(trimmedContent);
                
                // call Gemini API
                string summary = await CallGeminiApi(prompt);
                
                result.Summary = summary;
                
                // Check for common error patterns or generic responses
                bool hasError = summary.StartsWith("API Error:") || 
                                summary.StartsWith("Failed to parse") ||
                                summary.Contains("Please provide the research paper content") ||
                                summary.Contains("I need the text of the paper");
                                
                result.IsSuccess = !hasError;
                
                if (!result.IsSuccess && string.IsNullOrEmpty(result.ErrorMessage))
                {
                    result.ErrorMessage = "Unable to generate a proper summary from the provided content.";
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Summary generation error: {ex}");
                result.ErrorMessage = $"Error generating summary: {ex.Message}";
                result.IsSuccess = false;
                return result;
            }
        }
        
        private string CreatePrompt(string fileContent)
        {
            // Create a more specific prompt with clearer instructions
            return @"You are a research paper summarization assistant. Analyze and summarize the following research paper.
Focus specifically on:
1. The main research question or objective
2. The methodology used
3. Key findings and results
4. Important conclusions

Provide a concise but comprehensive summary that captures these key elements.
Here is the paper content:

" + fileContent;
        }
        
        private async Task<string> CallGeminiApi(string prompt)
        {
            try
            {
                // make API URL with key
                string apiUrlWithKey = $"{GeminiApiUrl}?key={GeminiApiKey}";
                Console.WriteLine($"Calling Gemini API at: {GeminiApiUrl}");
                
                // create request body with more specific safety settings and configuration
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
                        temperature = 0.1, // Lower temperature for more focused responses
                        maxOutputTokens = 2048,
                        topK = 40,
                        topP = 0.95
                    },
                    safetySettings = new[]
                    {
                        new
                        {
                            category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                            threshold = "BLOCK_NONE"
                        }
                    }
                };
                
                // send to Gemini API
                var response = await _httpClient.PostAsJsonAsync(apiUrlWithKey, requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response: {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}...");
                    
                    var jsonResponse = JsonDocument.Parse(responseContent).RootElement;
                    
                    // parse response to get summary
                    try
                    {
                        var candidates = jsonResponse.GetProperty("candidates");
                        var firstCandidate = candidates[0];
                        var content = firstCandidate.GetProperty("content");
                        var parts = content.GetProperty("parts");
                        var firstPart = parts[0];
                        var text = firstPart.GetProperty("text").GetString();
                        
                        // Check if the response is the generic "Please provide" message
                        if (string.IsNullOrEmpty(text) || 
                            text.Contains("Please provide the research paper content") ||
                            text.Contains("I need the text of the paper"))
                        {
                            Console.WriteLine("Received generic response from API. Trying alternative endpoint.");
                            return await TryAlternativeApiEndpoint(prompt);
                        }
                        
                        return text ?? "No text returned from API";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to parse API response: {ex.Message}");
                        return $"Failed to parse API response: {ex.Message}";
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Gemini API Error: {response.StatusCode} - {errorResponse}");
                    
                    // try backup API if first fails
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound || 
                        response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                        errorResponse.Contains("safety"))
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
                Console.WriteLine($"Calling alternate API at: {alternativeUrl}");
                
                // Using the same improved request body structure as the primary endpoint
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
                        temperature = 0.1,
                        maxOutputTokens = 2048,
                        topK = 40,
                        topP = 0.95
                    },
                    safetySettings = new[]
                    {
                        new
                        {
                            category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                            threshold = "BLOCK_NONE"
                        }
                    }
                };
                
                var response = await _httpClient.PostAsJsonAsync(apiUrlWithKey, requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Alternate API Response: {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}...");
                    
                    var jsonResponse = JsonDocument.Parse(responseContent).RootElement;
                    
                    // parse response to get summary
                    try
                    {
                        var candidates = jsonResponse.GetProperty("candidates");
                        var firstCandidate = candidates[0];
                        var content = firstCandidate.GetProperty("content");
                        var parts = content.GetProperty("parts");
                        var firstPart = parts[0];
                        var text = firstPart.GetProperty("text").GetString();
                        
                        // Check if the response is the generic "Please provide" message
                        if (string.IsNullOrEmpty(text) || 
                            text.Contains("Please provide the research paper content") ||
                            text.Contains("I need the text of the paper"))
                        {
                            // If even our backup API is returning generic responses,
                            // create a fallback summary that at least gives some information
                            Console.WriteLine("Received generic response from alternate API too. Falling back to basic summary.");
                            return CreateFallbackSummary();
                        }
                        
                        return text ?? "No text returned from alternate API";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to parse alternate API response: {ex.Message}");
                        return CreateFallbackSummary();
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Alternate API Error: {response.StatusCode} - {errorResponse}");
                    return CreateFallbackSummary();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception calling alternate API: {ex.Message}");
                return CreateFallbackSummary();
            }
        }
        
        private string CreateFallbackSummary()
        {
            // Provide a useful message when both APIs fail
            return "Unable to generate an automatic summary at this time. " +
                   "The paper has been successfully processed and its content is available " +
                   "in the 'Paper Content' tab. You can read the original text there.\n\n" +
                   "Possible reasons for this issue:\n" +
                   "- The API service may be temporarily unavailable\n" +
                   "- The paper format may not be compatible with our summarization tool\n" +
                   "- The content may be too specialized for automatic summarization\n\n" +
                   "Please try again later or with a different document.";
        }
    }
}