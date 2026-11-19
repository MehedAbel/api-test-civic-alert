using CivicAlert.Api.Data;
using CivicAlert.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Api.Services;

public class UserService(CivicAlertDbContext db)
{
    public async Task<AuthUserDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        if (user is null) return null;

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.Neighborhood = request.Neighborhood?.Trim();
        user.Bio = request.Bio?.Trim();
        if (!string.IsNullOrWhiteSpace(request.PreferredTheme))
        {
            user.PreferredTheme = request.PreferredTheme;
        }
        user.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return AuthService.MapUser(user);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        if (user is null) return (false, "User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return (false, "Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<IReadOnlyList<AdminUserSummaryDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        return await db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new AdminUserSummaryDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<AdminUserSummaryDto?> AssignRoleAsync(Guid targetUserId, AssignUserRoleRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct);
        if (user is null) return null;

        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new AdminUserSummaryDto(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.CreatedAtUtc);
    }
}
