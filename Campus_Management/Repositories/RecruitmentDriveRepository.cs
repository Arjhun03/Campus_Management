using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class RecruitmentDriveRepository : IRecruitmentDriveRepository
{
    private readonly ApplicationDbContext _context;

    public RecruitmentDriveRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RecruitmentDrive?> GetByIdAsync(int id)
    {
        return await _context.RecruitmentDrives
            .FirstOrDefaultAsync(d => d.DriveId == id);
    }

    public async Task<List<RecruitmentDrive>> GetByJobIdAsync(int jobId)
    {
        return await _context.RecruitmentDrives
            .Where(d => d.JobId == jobId)
            .OrderByDescending(d => d.DriveDate)
            .ToListAsync();
    }

    public async Task<List<RecruitmentDrive>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.RecruitmentDrives
            .Where(d => d.CompanyId == companyId)
            .OrderByDescending(d => d.DriveDate)
            .ToListAsync();
    }

    public async Task<List<RecruitmentDrive>> GetAllAsync()
    {
        return await _context.RecruitmentDrives
            .OrderByDescending(d => d.DriveDate)
            .ToListAsync();
    }

    public async Task<RecruitmentDrive> AddAsync(RecruitmentDrive drive)
    {
        await _context.RecruitmentDrives.AddAsync(drive);
        await _context.SaveChangesAsync();

        return drive;
    }

    public async Task UpdateAsync(RecruitmentDrive drive)
    {
        _context.RecruitmentDrives.Update(drive);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RecruitmentDrive drive)
    {
        _context.RecruitmentDrives.Remove(drive);
        await _context.SaveChangesAsync();
    }
}
