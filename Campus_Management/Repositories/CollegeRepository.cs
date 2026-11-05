using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class CollegeRepository : ICollegeRepository
{
    private readonly ApplicationDbContext _context;

    public CollegeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<College>> GetAllAsync()
    {
        return await _context.Colleges
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<College?> GetByIdAsync(int collegeId)
    {
        return await _context.Colleges
            .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
    }

    public async Task<College> AddAsync(College college)
    {
        await _context.Colleges.AddAsync(college);
        await _context.SaveChangesAsync();

        return college;
    }

    public async Task UpdateAsync(College college)
    {
        _context.Colleges.Update(college);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(College college)
    {
        _context.Colleges.Remove(college);
        await _context.SaveChangesAsync();
    }
}