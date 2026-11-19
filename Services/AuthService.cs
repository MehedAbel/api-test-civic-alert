using CivicAlert.Api.Data;
using CivicAlert.Api.DTOs;
using CivicAlert.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Api.Services;

public class AuthService(CivicAlertDbContext db, JwtService jwtService)
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && u.IsActive, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var (token, expiresAtUtc) = jwtService.GenerateToken(user);
        return new AuthResponse(token, expiresAtUtc, MapUser(user));
    }

    public async Task<(AuthResponse? Response, string? Error)> RegisterCitizenAsync(
        RegisterCitizenRequest request,
        CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct);
        if (exists)
        {
            return (null, "An account with this email address already exists.");
        }

        // Public registration is strictly restricted to Citizen accounts.
        // Officials and Admins must be assigned by an administrator.
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Citizen,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Neighborhood = request.Neighborhood?.Trim(),
            PreferredTheme = "light",
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var (token, expiresAtUtc) = jwtService.GenerateToken(user);
        return (new AuthResponse(token, expiresAtUtc, MapUser(user)), null);
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        return user is null ? null : MapUser(user);
    }

    internal static AuthUserDto MapUser(User user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.Role,
        user.PhoneNumber,
        user.Neighborhood,
        user.Bio,
        user.PreferredTheme,
        user.CreatedAtUtc
    );
}
