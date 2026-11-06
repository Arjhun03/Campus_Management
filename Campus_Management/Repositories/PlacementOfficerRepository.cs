using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class PlacementOfficerRepository : IPlacementOfficerRepository
{
    private readonly ApplicationDbContext _context;

    public PlacementOfficerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlacementOfficer>> GetAllAsync()
    {
        return await _context.PlacementOfficers
            .AsNoTracking()
            .Include(p => p.College)
            .ToListAsync();
    }

    public async Task<PlacementOfficer?> GetByIdAsync(int placementOfficerId)
    {
        return await _context.PlacementOfficers
            .Include(p => p.College)
            .FirstOrDefaultAsync(p =>
                p.PlacementOfficerId == placementOfficerId);
    }

    public async Task<PlacementOfficer?> GetByEmailAsync(string email)
    {
        return await _context.PlacementOfficers
            .Include(p => p.College)
            .FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<PlacementOfficer> AddAsync(
        PlacementOfficer placementOfficer)
    {
        await _context.PlacementOfficers.AddAsync(placementOfficer);
        await _context.SaveChangesAsync();

        return placementOfficer;
    }

    public async Task UpdateAsync(PlacementOfficer placementOfficer)
    {
        _context.PlacementOfficers.Update(placementOfficer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(PlacementOfficer placementOfficer)
    {
        _context.PlacementOfficers.Remove(placementOfficer);
        await _context.SaveChangesAsync();
    }
}
