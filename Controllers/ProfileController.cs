using System.Security.Claims;
using CivicAlert.Api.DTOs;
using CivicAlert.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicAlert.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController(AuthService authService, UserService userService) : ControllerBase
{
    /// <summary>
    /// Retrieve the current authenticated user's profile.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthUserDto>> GetProfile(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await authService.GetCurrentUserAsync(userId, ct);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Update personal details and theme preference for the authenticated user.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthUserDto>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var updated = await userService.UpdateProfileAsync(userId, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Change the authenticated user's password.
    /// </summary>
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var (success, error) = await userService.ChangePasswordAsync(userId, request, ct);
        return success ? NoContent() : BadRequest(new { message = error });
    }

    private bool TryGetUserId(out Guid userId)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out userId);
    }
}
