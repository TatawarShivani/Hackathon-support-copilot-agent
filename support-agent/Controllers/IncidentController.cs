using Microsoft.AspNetCore.Mvc;
using SupportAgent.Models;
using SupportAgent.Services;

namespace SupportAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentController : ControllerBase
{
    private readonly IAgentOrchestrator _orchestrator;
    private readonly ILogger<IncidentController> _logger;

    public IncidentController(IAgentOrchestrator orchestrator, ILogger<IncidentController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpGet("analyze/{incidentId}")]
    public async Task<ActionResult<IncidentAnalysisResponse>> AnalyzeIncident(string incidentId)
    {
        _logger.LogInformation("Received analysis request for incident {IncidentId}", incidentId);

        if (string.IsNullOrWhiteSpace(incidentId))
        {
            return BadRequest("Incident ID is required");
        }

        var analysis = await _orchestrator.AnalyzeIncidentAsync(incidentId);

        if (analysis == null)
        {
            return NotFound($"Incident {incidentId} not found");
        }

        return Ok(analysis);
    }
}
