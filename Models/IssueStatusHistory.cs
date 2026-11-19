using System.ComponentModel.DataAnnotations;

namespace CivicAlert.Api.Models;

public class IssueStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IssueReportId { get; set; }

    public IssueReport IssueReport { get; set; } = null!;

    public IssueStatus Status { get; set; }

    [MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    public Guid ChangedByUserId { get; set; }

    [MaxLength(120)]
    public string ChangedByName { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
