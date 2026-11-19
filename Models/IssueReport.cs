using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CivicAlert.Api.Models;

public class IssueReport
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(24)]
    public string ReferenceCode { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public IssueType IssueType { get; set; }

    public IssueStatus Status { get; set; } = IssueStatus.Submitted;

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    [MaxLength(240)]
    public string Address { get; set; } = string.Empty;

    public Guid ReporterId { get; set; }

    public User Reporter { get; set; } = null!;

    public Guid? AssignedOfficialId { get; set; }

    public User? AssignedOfficial { get; set; }

    [MaxLength(1000)]
    public string? OfficialNotes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<IssueImage> Images { get; set; } = new List<IssueImage>();

    public ICollection<IssueStatusHistory> StatusHistory { get; set; } = new List<IssueStatusHistory>();
}
