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

    public DbSet<PlacementOfficer> PlacementOfficers { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
}