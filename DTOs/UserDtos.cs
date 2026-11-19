using System.ComponentModel.DataAnnotations;
using CivicAlert.Api.Models;

namespace CivicAlert.Api.DTOs;

public record UpdateProfileRequest(
    [Required, MinLength(2), MaxLength(120)] string FullName,
    [MaxLength(32)] string? PhoneNumber,
    [MaxLength(160)] string? Neighborhood,
    [MaxLength(500)] string? Bio,
    [RegularExpression("^(light|dark)$")] string? PreferredTheme
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8), MaxLength(100)] string NewPassword
);

public record AdminUserSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAtUtc
);

public record AssignUserRoleRequest(
    [Required] UserRole Role,
    bool IsActive = true
);
