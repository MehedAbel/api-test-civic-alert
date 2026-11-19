using System.ComponentModel.DataAnnotations;

namespace CivicAlert.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Citizen;

    [MaxLength(32)]
    public string? PhoneNumber { get; set; }

    [MaxLength(160)]
    public string? Neighborhood { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(16)]
    public string PreferredTheme { get; set; } = "light";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<IssueReport> ReportedIssues { get; set; } = new List<IssueReport>();
}
