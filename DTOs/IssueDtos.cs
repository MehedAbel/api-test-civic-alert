using System.ComponentModel.DataAnnotations;
using CivicAlert.Api.Models;

namespace CivicAlert.Api.DTOs;

public record IssueImageDto(
    Guid Id,
    string ImageUrl,
    string FileName,
    int DisplayOrder
);

public record IssueStatusHistoryDto(
    Guid Id,
    IssueStatus Status,
    string Note,
    string ChangedByName,
    DateTime ChangedAtUtc
);

public record IssueReportDto(
    Guid Id,
    string ReferenceCode,
    string Title,
    string Description,
    IssueType IssueType,
    IssueStatus Status,
    decimal Latitude,
    decimal Longitude,
    string Address,
    Guid ReporterId,
    string ReporterName,
    string? OfficialNotes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<IssueImageDto> Images,
    IReadOnlyList<IssueStatusHistoryDto> StatusHistory
);

public record CreateIssueRequest(
    [Required, MinLength(3), MaxLength(160)] string Title,
    [Required, MinLength(10), MaxLength(2000)] string Description,
    [Required] IssueType IssueType,
    [Range(-90, 90)] decimal Latitude,
    [Range(-180, 180)] decimal Longitude,
    [MaxLength(240)] string Address
);

public record UpdateIssueStatusRequest(
    [Required] IssueStatus Status,
    [MaxLength(1000)] string? Note
);
