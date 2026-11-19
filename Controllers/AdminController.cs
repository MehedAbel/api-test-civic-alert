using CivicAlert.Api.DTOs;
using CivicAlert.Api.Models;
using CivicAlert.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicAlert.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(UserRole.Admin))]
[Produces("application/json")]
public class AdminController(UserService userService) : ControllerBase
{
    /// <summary>
    /// List all registered accounts for system administration and role assignment.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminUserSummaryDto>>> GetUsers(CancellationToken ct)
    {
        var users = await userService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    /// <summary>
    /// Manually assign Citizen, Official, or Admin role to an account.
    /// </summary>
    [HttpPatch("users/{id:guid}/role")]
    [ProducesResponseType(typeof(AdminUserSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserSummaryDto>> AssignRole(
        Guid id,
        [FromBody] AssignUserRoleRequest request,
        CancellationToken ct)
    {
        var updated = await userService.AssignRoleAsync(id, request, ct);
        return updated is null
            ? NotFound(new { message = "User not found." })
            : Ok(updated);
    }
}
