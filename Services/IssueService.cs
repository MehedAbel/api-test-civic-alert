using CivicAlert.Api.Data;
using CivicAlert.Api.DTOs;
using CivicAlert.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Api.Services;

public class IssueService(CivicAlertDbContext db, StorageService storageService)
{
    public async Task<IReadOnlyList<IssueReportDto>> GetAllAsync(
        IssueType? type,
        IssueStatus? status,
        CancellationToken ct = default)
    {
        var query = BaseIssueQuery();

        if (type.HasValue)
            query = query.Where(i => i.IssueType == type.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        var items = await query.OrderByDescending(i => i.CreatedAtUtc).ToListAsync(ct);
        return items.Select(MapIssue).ToList();
    }

    public async Task<IReadOnlyList<IssueReportDto>> GetByReporterAsync(Guid reporterId, CancellationToken ct = default)
    {
        var items = await BaseIssueQuery()
            .Where(i => i.ReporterId == reporterId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(ct);

        return items.Select(MapIssue).ToList();
    }

    public async Task<IssueReportDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await BaseIssueQuery().FirstOrDefaultAsync(i => i.Id == id, ct);
        return item is null ? null : MapIssue(item);
    }

    public async Task<IssueReportDto> CreateAsync(
        Guid reporterId,
        CreateIssueRequest request,
        IReadOnlyList<IFormFile> images,
        CancellationToken ct = default)
    {
        var reporter = await db.Users.FirstOrDefaultAsync(u => u.Id == reporterId, ct)
            ?? throw new InvalidOperationException("Reporter account not found.");

        var count = await db.IssueReports.CountAsync(ct);
        var refCode = $"CA-{DateTime.UtcNow.Year}-{1041 + count}";

        var report = new IssueReport
        {
            ReferenceCode = refCode,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            IssueType = request.IssueType,
            Status = IssueStatus.Submitted,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = string.IsNullOrWhiteSpace(request.Address) ? $"{request.Latitude:F5}, {request.Longitude:F5}" : request.Address.Trim(),
            ReporterId = reporterId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        for (var idx = 0; idx < images.Count; idx++)
        {
            var imageUrl = await storageService.SaveImageAsync(images[idx], ct);
            report.Images.Add(new IssueImage
            {
                ImageUrl = imageUrl,
                FileName = images[idx].FileName,
                DisplayOrder = idx
            });
        }

        report.StatusHistory.Add(new IssueStatusHistory
        {
            Status = IssueStatus.Submitted,
            Note = "Report submitted via Citizen Map.",
            ChangedByUserId = reporter.Id,
            ChangedByName = reporter.FullName,
            ChangedAtUtc = DateTime.UtcNow
        });

        db.IssueReports.Add(report);
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(report.Id, ct))!;
    }

    public async Task<IssueReportDto?> UpdateStatusAsync(
        Guid id,
        Guid actorId,
        UpdateIssueStatusRequest request,
        CancellationToken ct = default)
    {
        var report = await db.IssueReports
            .Include(i => i.StatusHistory)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (report is null) return null;

        var actor = await db.Users.FirstOrDefaultAsync(u => u.Id == actorId, ct)
            ?? throw new InvalidOperationException("Actor account not found.");

        report.Status = request.Status;
        report.OfficialNotes = request.Note?.Trim() ?? report.OfficialNotes;
        report.AssignedOfficialId ??= actor.Id;
        report.UpdatedAtUtc = DateTime.UtcNow;

        report.StatusHistory.Add(new IssueStatusHistory
        {
            Status = request.Status,
            Note = string.IsNullOrWhiteSpace(request.Note) ? $"Status updated to {request.Status}." : request.Note.Trim(),
            ChangedByUserId = actor.Id,
            ChangedByName = actor.FullName,
            ChangedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(report.Id, ct);
    }

    private IQueryable<IssueReport> BaseIssueQuery() =>
        db.IssueReports
            .AsNoTracking()
            .Include(i => i.Reporter)
            .Include(i => i.Images.OrderBy(img => img.DisplayOrder))
            .Include(i => i.StatusHistory.OrderBy(h => h.ChangedAtUtc));

    private static IssueReportDto MapIssue(IssueReport report) => new(
        report.Id,
        report.ReferenceCode,
        report.Title,
        report.Description,
        report.IssueType,
        report.Status,
        report.Latitude,
        report.Longitude,
        report.Address,
        report.ReporterId,
        report.Reporter.FullName,
        report.OfficialNotes,
        report.CreatedAtUtc,
        report.UpdatedAtUtc,
        report.Images.OrderBy(img => img.DisplayOrder).Select(img => new IssueImageDto(img.Id, img.ImageUrl, img.FileName, img.DisplayOrder)).ToList(),
        report.StatusHistory.OrderBy(h => h.ChangedAtUtc).Select(h => new IssueStatusHistoryDto(h.Id, h.Status, h.Note, h.ChangedByName, h.ChangedAtUtc)).ToList()
    );
}
