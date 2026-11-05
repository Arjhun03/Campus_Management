using Campus_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<College> Colleges { get; set; } = null!;
}