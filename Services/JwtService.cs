using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CivicAlert.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace CivicAlert.Api.Services;

public class JwtService(IConfiguration configuration)
{
    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
    {
        var section = configuration.GetSection("JwtSettings");
        var secret = section["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing.");
        var issuer = section["Issuer"] ?? "CivicAlert.Api";
        var audience = section["Audience"] ?? "CivicAlert.Client";
        var expiryMinutes = int.TryParse(section["ExpiryMinutes"], out var mins) ? mins : 1440;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
