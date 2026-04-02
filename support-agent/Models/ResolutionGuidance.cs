namespace SupportAgent.Models;

public class ResolutionGuidance
{
    public string Summary { get; set; } = string.Empty;
    public List<ResolutionStep> Steps { get; set; } = new();
    public string SlaWarning { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
}

public class ResolutionStep
{
    public int StepNumber { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
}
