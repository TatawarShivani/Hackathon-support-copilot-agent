using Microsoft.AspNetCore.Mvc;
using SupportAgent.Models;
using SupportAgent.Services;

namespace SupportAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentController : ControllerBase
{
    private readonly IJiraService _jiraService;
    private readonly ILogger<IncidentController> _logger;

    public IncidentController(IJiraService jiraService, ILogger<IncidentController> logger)
    {
        _jiraService = jiraService;
        _logger = logger;
    }

    [HttpGet("{incidentId}")]
    public async Task<ActionResult<IncidentDetails>> GetIncident(string incidentId)
    {
        _logger.LogInformation("Received request for incident {IncidentId}", incidentId);

        if (string.IsNullOrWhiteSpace(incidentId))
        {
            return BadRequest("Incident ID is required");
        }

        var incident = await _jiraService.GetIncidentAsync(incidentId);

        if (incident == null)
        {
            return NotFound($"Incident {incidentId} not found");
        }

        return Ok(incident);
    }
}
