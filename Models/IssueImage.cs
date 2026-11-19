using System.ComponentModel.DataAnnotations;

namespace CivicAlert.Api.Models;

public class IssueImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IssueReportId { get; set; }

    public IssueReport IssueReport { get; set; } = null!;

    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
