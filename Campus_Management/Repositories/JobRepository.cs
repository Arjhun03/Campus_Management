using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _context;

    public JobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobOpening?> GetByIdAsync(int id)
    {
        return await _context.JobOpenings
            .FirstOrDefaultAsync(j => j.JobId == id);
    }

    public async Task<List<JobOpening>> GetAllAsync()
    {
        return await _context.JobOpenings
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<JobOpening>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.JobOpenings
            .Where(j => j.CompanyId == companyId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<JobOpening> AddAsync(JobOpening job)
    {
        await _context.JobOpenings.AddAsync(job);
        await _context.SaveChangesAsync();

        return job;
    }

    public async Task UpdateAsync(JobOpening job)
    {
        _context.JobOpenings.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(JobOpening job)
    {
        _context.JobOpenings.Remove(job);
        await _context.SaveChangesAsync();
    }
}