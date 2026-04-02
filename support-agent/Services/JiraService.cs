using System.Diagnostics;
using System.Text.RegularExpressions;
using SupportAgent.Models;

namespace SupportAgent.Services;

public interface IJiraService
{
    Task<IncidentDetails?> GetIncidentAsync(string incidentId);
}

public class JiraService : IJiraService
{
    private readonly ILogger<JiraService> _logger;

    public JiraService(ILogger<JiraService> logger)
    {
        _logger = logger;
    }

    public async Task<IncidentDetails?> GetIncidentAsync(string incidentId)
    {
        try
        {
            _logger.LogInformation("Fetching incident {IncidentId} from JIRA", incidentId);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "acli",
                    Arguments = $"jira workitem view {incidentId}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("ACLI error: {Error}", error);
                return null;
            }

            // Parse the output
            var incident = ParseJiraOutput(output);
            incident.Key = incidentId;

            return incident;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching incident {IncidentId}", incidentId);
            return null;
        }
    }

    private IncidentDetails ParseJiraOutput(string output)
    {
        var incident = new IncidentDetails();

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;

            var key = parts[0];
            var value = parts[1];

            switch (key)
            {
                case "Type":
                    incident.Type = value;
                    break;
                case "Summary":
                    incident.Summary = value;
                    break;
                case "Status":
                    incident.Status = value;
                    break;
                case "Assignee":
                    incident.Assignee = value;
                    break;
                case "Description":
                    incident.Description = value;
                    break;
                case "Priority":
                    incident.Priority = value;
                    break;
            }
        }

        return incident;
    }
}
