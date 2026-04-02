using System.Text;
using System.Text.Json;
using SupportAgent.Models;

namespace SupportAgent.Services;

public interface IClaudeService
{
    Task<ResolutionGuidance?> AnalyzeIncidentAsync(IncidentDetails incident, List<SimilarIncident> similarIncidents);
}

public class ClaudeService : IClaudeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeService> _logger;
    private readonly HttpClient _httpClient;

    public ClaudeService(IConfiguration configuration, ILogger<ClaudeService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<ResolutionGuidance?> AnalyzeIncidentAsync(IncidentDetails incident, List<SimilarIncident> similarIncidents)
    {
        try
        {
            var apiKey = _configuration["Claude:ApiKey"] ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Claude API key not configured");
                return null;
            }

            var prompt = BuildAnalysisPrompt(incident, similarIncidents);

            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 2000,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            _logger.LogInformation("Sending request to Claude API");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return null;
            }

            var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseContent);

            if (claudeResponse?.Content == null || claudeResponse.Content.Length == 0)
            {
                _logger.LogWarning("Empty response from Claude API");
                return null;
            }

            var analysisText = claudeResponse.Content[0].Text;
            return ParseClaudeResponse(analysisText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            return null;
        }
    }

    private string BuildAnalysisPrompt(IncidentDetails incident, List<SimilarIncident> similarIncidents)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a production support AI assistant. Analyze this incident and provide actionable resolution guidance.");
        sb.AppendLine();
        sb.AppendLine("## Current Incident");
        sb.AppendLine($"ID: {incident.Key}");
        sb.AppendLine($"Type: {incident.Type}");
        sb.AppendLine($"Priority: {incident.Priority}");
        sb.AppendLine($"Status: {incident.Status}");
        sb.AppendLine($"Summary: {incident.Summary}");
        sb.AppendLine($"Description: {incident.Description}");
        sb.AppendLine();

        if (similarIncidents.Any())
        {
            sb.AppendLine("## Similar Past Incidents");
            foreach (var similar in similarIncidents.Take(3))
            {
                sb.AppendLine($"- {similar.Key}: {similar.Summary} (Status: {similar.Status})");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Instructions");
        sb.AppendLine("Provide your analysis in the following JSON format:");
        sb.AppendLine(@"{
  ""summary"": ""Brief diagnosis of the issue"",
  ""steps"": [
    {
      ""stepNumber"": 1,
      ""action"": ""Description of what to do"",
      ""command"": ""Actual command to run (if applicable)"",
      ""expectedOutput"": ""What should happen""
    }
  ],
  ""slaWarning"": ""SLA-related warning if priority is high"",
  ""confidenceScore"": 0.85
}");
        sb.AppendLine();
        sb.AppendLine("Focus on:");
        sb.AppendLine("1. Root cause analysis based on the description");
        sb.AppendLine("2. Specific actionable steps with commands where applicable");
        sb.AppendLine("3. Validation checks after each step");
        sb.AppendLine("4. If priority is High or Critical, include SLA warning");
        sb.AppendLine();
        sb.AppendLine("Return ONLY the JSON, no additional text.");

        return sb.ToString();
    }

    private ResolutionGuidance? ParseClaudeResponse(string responseText)
    {
        try
        {
            // Extract JSON from response (Claude sometimes adds markdown code blocks)
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');

            if (jsonStart == -1 || jsonEnd == -1)
            {
                _logger.LogWarning("No JSON found in Claude response");
                return null;
            }

            var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<ResolutionGuidance>(jsonText, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Claude response");
            return null;
        }
    }

    private class ClaudeApiResponse
    {
        public ClaudeContent[]? Content { get; set; }
    }

    private class ClaudeContent
    {
        public string Text { get; set; } = string.Empty;
    }
}
