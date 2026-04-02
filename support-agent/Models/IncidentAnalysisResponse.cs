namespace SupportAgent.Models;

public class IncidentAnalysisResponse
{
    public IncidentDetails IncidentSummary { get; set; } = new();
    public List<SimilarIncident> SimilarIncidents { get; set; } = new();
    public ResolutionGuidance? ResolutionGuidance { get; set; }
}

public class SimilarIncident
{
    public string Key { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Relevance { get; set; }
}
