using CivicAlert.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(CivicAlertDbContext db, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        var citizen1 = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "Elena Vance",
            Email = "citizen@civicalert.org",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Citizen123!"),
            Role = UserRole.Citizen,
            PhoneNumber = "+1 (555) 234-8910",
            Neighborhood = "Oakwood District",
            Bio = "Daily commuter and neighborhood street safety volunteer.",
            PreferredTheme = "light"
        };

        var official1 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FullName = "Marcus Sterling",
            Email = "official@civicalert.org",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Official123!"),
            Role = UserRole.Official,
            PhoneNumber = "+1 (555) 900-4411",
            Neighborhood = "Municipal Public Works",
            Bio = "Senior Infrastructure Operations Lead.",
            PreferredTheme = "light"
        };

        var admin1 = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FullName = "Nadia Kovalenko",
            Email = "admin@civicalert.org",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            PhoneNumber = "+1 (555) 900-0001",
            Neighborhood = "Civic Alert Systems",
            Bio = "System administrator managing official and admin role provisioning.",
            PreferredTheme = "dark"
        };

        db.Users.AddRange(citizen1, official1, admin1);

        var report1 = new IssueReport
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            ReferenceCode = "CA-2026-1041",
            Title = "Deep asphalt pothole along Elm & 4th Ave",
            Description = "Large crater near the eastbound curb collecting rainwater. Vehicles are swerving into the bike lane to avoid wheel damage.",
            IssueType = IssueType.Pothole,
            Status = IssueStatus.InProgress,
            Latitude = 51.509865m,
            Longitude = -0.118092m,
            Address = "412 Elm Street, Central District",
            ReporterId = citizen1.Id,
            AssignedOfficialId = official1.Id,
            OfficialNotes = "Crews scheduled for cold-patch stabilization and resurfacing.",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-3),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-8),
            Images =
            [
                new IssueImage { ImageUrl = "/src/assets/images/issue_pothole_asphalt_1794926544987.jpg", FileName = "pothole-overview.jpg", DisplayOrder = 0 },
                new IssueImage { ImageUrl = "/src/assets/images/issue_pothole_closeup_1794926545214.jpg", FileName = "pothole-cone-closeup.jpg", DisplayOrder = 1 }
            ],
            StatusHistory =
            [
                new IssueStatusHistory { Status = IssueStatus.Submitted, Note = "Report filed by citizen.", ChangedByUserId = citizen1.Id, ChangedByName = citizen1.FullName, ChangedAtUtc = DateTime.UtcNow.AddDays(-3) },
                new IssueStatusHistory { Status = IssueStatus.UnderReview, Note = "Verified photo evidence and prioritized.", ChangedByUserId = official1.Id, ChangedByName = official1.FullName, ChangedAtUtc = DateTime.UtcNow.AddDays(-2) },
                new IssueStatusHistory { Status = IssueStatus.InProgress, Note = "Road crew dispatched with safety cone perimeter.", ChangedByUserId = official1.Id, ChangedByName = official1.FullName, ChangedAtUtc = DateTime.UtcNow.AddHours(-8) }
            ]
        };

        var report2 = new IssueReport
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            ReferenceCode = "CA-2026-1042",
            Title = "Unlit streetlight & loose luminaire cover",
            Description = "Streetlamp outside 88 Market Terrace failed completely last night and the glass diffuser cover is hanging loose.",
            IssueType = IssueType.BrokenStreetlight,
            Status = IssueStatus.UnderReview,
            Latitude = 51.512500m,
            Longitude = -0.125200m,
            Address = "88 Market Terrace, West End",
            ReporterId = citizen1.Id,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-5),
            Images =
            [
                new IssueImage { ImageUrl = "/src/assets/images/issue_streetlight_broken_1794926546649.jpg", FileName = "streetlight-dusk.jpg", DisplayOrder = 0 },
                new IssueImage { ImageUrl = "/src/assets/images/issue_streetlight_wiring_1794926546097.jpg", FileName = "luminaire-cover.jpg", DisplayOrder = 1 }
            ],
            StatusHistory =
            [
                new IssueStatusHistory { Status = IssueStatus.Submitted, Note = "Report filed by citizen.", ChangedByUserId = citizen1.Id, ChangedByName = citizen1.FullName, ChangedAtUtc = DateTime.UtcNow.AddDays(-1) },
                new IssueStatusHistory { Status = IssueStatus.UnderReview, Note = "Queued for electrical inspection.", ChangedByUserId = official1.Id, ChangedByName = official1.FullName, ChangedAtUtc = DateTime.UtcNow.AddHours(-5) }
            ]
        };

        db.IssueReports.AddRange(report1, report2);
        await db.SaveChangesAsync(ct);
    }
}
