using CivicAlert.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Api.Data;

public class CivicAlertDbContext(DbContextOptions<CivicAlertDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<IssueReport> IssueReports => Set<IssueReport>();
    public DbSet<IssueImage> IssueImages => Set<IssueImage>();
    public DbSet<IssueStatusHistory> IssueStatusHistories => Set<IssueStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Role);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(24);
        });

        modelBuilder.Entity<IssueReport>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.ReferenceCode).IsUnique();
            entity.HasIndex(i => i.IssueType);
            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.CreatedAtUtc);

            entity.Property(i => i.IssueType).HasConversion<string>().HasMaxLength(32);
            entity.Property(i => i.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(i => i.Latitude).HasPrecision(9, 6);
            entity.Property(i => i.Longitude).HasPrecision(9, 6);

            entity.HasOne(i => i.Reporter)
                .WithMany(u => u.ReportedIssues)
                .HasForeignKey(i => i.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.AssignedOfficial)
                .WithMany()
                .HasForeignKey(i => i.AssignedOfficialId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IssueImage>(entity =>
        {
            entity.HasKey(img => img.Id);
            entity.HasOne(img => img.IssueReport)
                .WithMany(i => i.Images)
                .HasForeignKey(img => img.IssueReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IssueStatusHistory>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Status).HasConversion<string>().HasMaxLength(24);
            entity.HasOne(h => h.IssueReport)
                .WithMany(i => i.StatusHistory)
                .HasForeignKey(h => h.IssueReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
