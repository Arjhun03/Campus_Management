using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class JobTargetCollegeRepository : IJobTargetCollegeRepository
{
    private readonly ApplicationDbContext _context;

    public JobTargetCollegeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobTargetCollege>> GetByJobIdAsync(int jobId)
    {
        return await _context.JobTargetColleges
            .Where(x => x.JobId == jobId)
            .OrderBy(x => x.CollegeId)
            .ToListAsync();
    }

    public async Task<JobTargetCollege?> GetAsync(
        int jobId,
        int collegeId)
    {
        return await _context.JobTargetColleges
            .FirstOrDefaultAsync(x =>
                x.JobId == jobId &&
                x.CollegeId == collegeId);
    }

    public async Task<bool> ExistsAsync(
        int jobId,
        int collegeId)
    {
        return await _context.JobTargetColleges
            .AnyAsync(x =>
                x.JobId == jobId &&
                x.CollegeId == collegeId);
    }

    public async Task<JobTargetCollege> AddAsync(
        JobTargetCollege target)
    {
        await _context.JobTargetColleges.AddAsync(target);
        await _context.SaveChangesAsync();

        return target;
    }

    public async Task DeleteAsync(
        JobTargetCollege target)
    {
        _context.JobTargetColleges.Remove(target);
        await _context.SaveChangesAsync();
    }
}