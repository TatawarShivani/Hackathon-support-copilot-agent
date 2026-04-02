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
        sb.AppendLine("You are an expert production support engineer analyzing a technical incident. Provide specific, actionable resolution steps.");
        sb.AppendLine();
        sb.AppendLine("## Current Incident");
        sb.AppendLine($"**ID:** {incident.Key}");
        sb.AppendLine($"**Type:** {incident.Type}");
        sb.AppendLine($"**Priority:** {incident.Priority}");
        sb.AppendLine($"**Status:** {incident.Status}");
        sb.AppendLine($"**Summary:** {incident.Summary}");
        sb.AppendLine($"**Description:** {incident.Description}");
        sb.AppendLine();

        if (similarIncidents.Any())
        {
            sb.AppendLine("## Similar Resolved Incidents (for context)");
            foreach (var similar in similarIncidents.Take(3))
            {
                sb.AppendLine($"- **{similar.Key}:** {similar.Summary} ({similar.Status})");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Task");
        sb.AppendLine("Analyze the incident description carefully. Identify:");
        sb.AppendLine("1. The likely root cause based on error messages, symptoms, and context");
        sb.AppendLine("2. Specific technical steps to diagnose and resolve");
        sb.AppendLine("3. Exact commands, file paths, or configuration changes needed");
        sb.AppendLine("4. Validation steps to confirm resolution");
        sb.AppendLine();
        sb.AppendLine("## Output Format");
        sb.AppendLine("Return a JSON object with this exact structure:");
        sb.AppendLine(@"{
  ""summary"": ""<2-3 sentence diagnosis explaining the root cause and recommended fix>"",
  ""steps"": [
    {
      ""stepNumber"": 1,
      ""action"": ""<Clear action description>"",
      ""command"": ""<Exact command to run, or empty string if not applicable>"",
      ""expectedOutput"": ""<What to expect after running the command>""
    }
  ],
  ""slaWarning"": ""<Warning about time-sensitive actions if priority is High/Critical, or empty string>"",
  ""confidenceScore"": <number between 0.0 and 1.0>
}");
        sb.AppendLine();
        sb.AppendLine("## Guidelines");
        sb.AppendLine("- Be specific: Use actual file paths, service names, command syntax");
        sb.AppendLine("- For SQL/database issues: Suggest checking connections, timeouts, query performance");
        sb.AppendLine("- For file issues: Check file existence, permissions, size");
        sb.AppendLine("- For jobs: Check logs, schedules, dependencies");
        sb.AppendLine("- Include 3-5 actionable steps");
        sb.AppendLine("- Confidence score: 0.9+ if very similar to past incidents, 0.7-0.8 if clear diagnosis, 0.5-0.6 if more investigation needed");
        sb.AppendLine();
        sb.AppendLine("Return ONLY valid JSON, no markdown code blocks or extra text.");

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
