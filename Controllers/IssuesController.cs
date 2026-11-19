using System.Security.Claims;
using CivicAlert.Api.DTOs;
using CivicAlert.Api.Models;
using CivicAlert.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicAlert.Api.Controllers;

[ApiController]
[Route("api/issues")]
[Authorize]
[Produces("application/json")]
public class IssuesController(IssueService issueService) : ControllerBase
{
    /// <summary>
    /// Get all community issues for the interactive map with optional type and status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<IssueReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IssueReportDto>>> GetAll(
        [FromQuery] IssueType? type,
        [FromQuery] IssueStatus? status,
        CancellationToken ct)
    {
        var issues = await issueService.GetAllAsync(type, status, ct);
        return Ok(issues);
    }

    /// <summary>
    /// Get all issue reports submitted by the authenticated citizen.
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Citizen))]
    [ProducesResponseType(typeof(IReadOnlyList<IssueReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IssueReportDto>>> GetMine(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var issues = await issueService.GetByReporterAsync(userId, ct);
        return Ok(issues);
    }

    /// <summary>
    /// Get detailed information, photos, and status history for a specific issue.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IssueReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueReportDto>> GetById(Guid id, CancellationToken ct)
    {
        var issue = await issueService.GetByIdAsync(id, ct);
        return issue is null
            ? NotFound(new { message = "Issue report not found." })
            : Ok(issue);
    }

    /// <summary>
    /// Submit a new civic issue report by dropping a pin on the map and attaching images.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Citizen))]
    [ProducesResponseType(typeof(IssueReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IssueReportDto>> Create(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var form = await Request.ReadFormAsync(ct);

        var title = form["title"].ToString();
        var description = form["description"].ToString();
        var address = form["address"].ToString();

        if (!Enum.TryParse<IssueType>(form["issueType"], ignoreCase: true, out var issueType))
        {
            return BadRequest(new { message = "Invalid issueType value." });
        }

        if (!decimal.TryParse(form["latitude"], out var latitude) ||
            !decimal.TryParse(form["longitude"], out var longitude))
        {
            return BadRequest(new { message = "Valid latitude and longitude coordinates are required." });
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
        {
            return BadRequest(new { message = "Title and description are required." });
        }

        var request = new CreateIssueRequest(title, description, issueType, latitude, longitude, address);
        var files = form.Files.GetFiles("images").ToList();

        var created = await issueService.CreateAsync(userId, request, files, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update the status and official notes of a reported issue (Official / Admin only).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Official)},{nameof(UserRole.Admin)}")]
    [ProducesResponseType(typeof(IssueReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueReportDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateIssueStatusRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var actorId))
        {
            return Unauthorized();
        }

        var updated = await issueService.UpdateStatusAsync(id, actorId, request, ct);
        return updated is null
            ? NotFound(new { message = "Issue report not found." })
            : Ok(updated);
    }

    /// <summary>
    /// Retrieve municipal triage queue for Officials and Admins.
    /// </summary>
    [HttpGet("/api/official/queue")]
    [Authorize(Roles = $"{nameof(UserRole.Official)},{nameof(UserRole.Admin)}")]
    [ProducesResponseType(typeof(IReadOnlyList<IssueReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IssueReportDto>>> GetOfficialQueue(
        [FromQuery] IssueStatus? status,
        CancellationToken ct)
    {
        var issues = await issueService.GetAllAsync(null, status, ct);
        return Ok(issues);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out userId);
    }
}
