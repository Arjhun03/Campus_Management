using Campus_Management.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<College> Colleges { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<CompanyHR> CompanyHRs { get; set; } = null!;
    public DbSet<PlacementOfficer> PlacementOfficers { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;

    public DbSet<JobOpening> JobOpenings { get; set; } = null!;
    public DbSet<JobTargetCollege> JobTargetColleges { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Student>()
            .HasIndex(s => new
            {
                s.CollegeId,
                s.RegisterNumber
            })
            .IsUnique();

        builder.Entity<Student>()
            .HasOne<College>()
            .WithMany()
            .HasForeignKey(s => s.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Student>()
            .HasOne<PlacementOfficer>()
            .WithMany()
            .HasForeignKey(s => s.PlacementOfficerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Student>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JobOpening>()
            .HasIndex(j => j.CompanyId);

        builder.Entity<JobOpening>()
            .HasIndex(j => j.CreatedByHRId);

        builder.Entity<JobOpening>()
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JobOpening>()
            .HasOne<CompanyHR>()
            .WithMany()
            .HasForeignKey(j => j.CreatedByHRId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JobTargetCollege>()
            .HasIndex(j => new
            {
                j.JobId,
                j.CollegeId
            })
            .IsUnique();

        builder.Entity<JobTargetCollege>()
            .HasOne<JobOpening>()
            .WithMany()
            .HasForeignKey(j => j.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JobTargetCollege>()
            .HasOne<College>()
            .WithMany()
            .HasForeignKey(j => j.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}