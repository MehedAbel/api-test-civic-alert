using System.ComponentModel.DataAnnotations;
using CivicAlert.Api.Models;

namespace CivicAlert.Api.DTOs;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record RegisterCitizenRequest(
    [Required, MinLength(2), MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [MaxLength(32)] string? PhoneNumber,
    [MaxLength(160)] string? Neighborhood
);

public record AuthUserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    string? PhoneNumber,
    string? Neighborhood,
    string? Bio,
    string PreferredTheme,
    DateTime CreatedAtUtc
);

public record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AuthUserDto User
);
