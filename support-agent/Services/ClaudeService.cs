using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
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
    private readonly IAmazonBedrockRuntime _bedrockClient;

    public ClaudeService(IConfiguration configuration, ILogger<ClaudeService> logger, IAmazonBedrockRuntime bedrockClient)
    {
        _configuration = configuration;
        _logger = logger;
        _bedrockClient = bedrockClient;
    }

    public async Task<ResolutionGuidance?> AnalyzeIncidentAsync(IncidentDetails incident, List<SimilarIncident> similarIncidents)
    {
        try
        {
            var modelId = _configuration["AWS:BedrockModelId"] ?? "eu.anthropic.claude-sonnet-4-5-20250929-v1:0";
            var prompt = BuildAnalysisPrompt(incident, similarIncidents);

            _logger.LogInformation("Sending request to AWS Bedrock with model {ModelId}", modelId);

            var request = new ConverseRequest
            {
                ModelId = modelId,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = ConversationRole.User,
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock { Text = prompt }
                        }
                    }
                },
                InferenceConfig = new InferenceConfiguration
                {
                    MaxTokens = 2000,
                    Temperature = 0.7f
                }
            };

            var response = await _bedrockClient.ConverseAsync(request);

            if (response.Output?.Message?.Content == null || response.Output.Message.Content.Count == 0)
            {
                _logger.LogWarning("Empty response from AWS Bedrock");
                return null;
            }

            var analysisText = response.Output.Message.Content[0].Text;
            return ParseClaudeResponse(analysisText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AWS Bedrock");
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
}
