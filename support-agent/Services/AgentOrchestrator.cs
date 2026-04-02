using SupportAgent.Models;

namespace SupportAgent.Services;

public interface IAgentOrchestrator
{
    Task<IncidentAnalysisResponse?> AnalyzeIncidentAsync(string incidentId);
}

public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IJiraService _jiraService;
    private readonly IClaudeService _claudeService;
    private readonly ILogger<AgentOrchestrator> _logger;

    public AgentOrchestrator(
        IJiraService jiraService,
        IClaudeService claudeService,
        ILogger<AgentOrchestrator> logger)
    {
        _jiraService = jiraService;
        _claudeService = claudeService;
        _logger = logger;
    }

    public async Task<IncidentAnalysisResponse?> AnalyzeIncidentAsync(string incidentId)
    {
        _logger.LogInformation("Starting analysis for incident {IncidentId}", incidentId);

        // Step 1: Fetch incident details
        var incident = await _jiraService.GetIncidentAsync(incidentId);
        if (incident == null)
        {
            _logger.LogWarning("Incident {IncidentId} not found", incidentId);
            return null;
        }

        // Step 2: Search for similar incidents
        var similarIncidents = await SearchSimilarIncidentsAsync(incident);

        // Step 3: Get AI analysis
        var resolutionGuidance = await _claudeService.AnalyzeIncidentAsync(incident, similarIncidents);

        // Step 4: Build response
        var response = new IncidentAnalysisResponse
        {
            IncidentSummary = incident,
            SimilarIncidents = similarIncidents,
            ResolutionGuidance = resolutionGuidance
        };

        _logger.LogInformation("Analysis complete for incident {IncidentId}", incidentId);
        return response;
    }

    private async Task<List<SimilarIncident>> SearchSimilarIncidentsAsync(IncidentDetails incident)
    {
        try
        {
            // Extract keywords from summary for search
            var keywords = ExtractKeywords(incident.Summary);

            if (string.IsNullOrEmpty(keywords))
            {
                return new List<SimilarIncident>();
            }

            // Search JIRA for similar resolved incidents
            var searchResults = await _jiraService.SearchIncidentsAsync(keywords, maxResults: 5);

            return searchResults.Select((r, index) => new SimilarIncident
            {
                Key = r.Key,
                Summary = r.Summary,
                Status = r.Status,
                Relevance = 1.0 - (index * 0.1) // Simple relevance scoring
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for similar incidents");
            return new List<SimilarIncident>();
        }
    }

    private string ExtractKeywords(string summary)
    {
        // Simple keyword extraction - remove common words
        var commonWords = new[] { "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for" };

        var words = summary.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3 && !commonWords.Contains(w.ToLower()))
            .Take(3);

        return string.Join(" ", words);
    }
}
