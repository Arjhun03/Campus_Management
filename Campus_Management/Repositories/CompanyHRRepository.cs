using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class CompanyHRRepository : ICompanyHRRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyHRRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyHR>> GetAllAsync()
    {
        return await _context.CompanyHRs
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CompanyHR>> GetPendingAsync()
    {
        return await _context.CompanyHRs
            .AsNoTracking()
            .Where(hr => hr.ApprovedAt == null)
            .ToListAsync();
    }

    public async Task<CompanyHR?> GetByIdAsync(int id)
    {
        return await _context.CompanyHRs
            .AsNoTracking()
            .FirstOrDefaultAsync(hr => hr.CompanyHRId == id);
    }

    public async Task<CompanyHR?> GetByUserIdAsync(string userId)
    {
        return await _context.CompanyHRs
            .AsNoTracking()
            .FirstOrDefaultAsync(hr => hr.UserId == userId);
    }

    public async Task<CompanyHR> CreateAsync(CompanyHR companyHR)
    {
        await _context.CompanyHRs.AddAsync(companyHR);
        await _context.SaveChangesAsync();

        return companyHR;
    }

    public async Task<bool> UpdateAsync(CompanyHR companyHR)
    {
        var existingCompanyHR = await _context.CompanyHRs
            .FirstOrDefaultAsync(
                hr => hr.CompanyHRId == companyHR.CompanyHRId);

        if (existingCompanyHR == null)
        {
            return false;
        }

        existingCompanyHR.UserId = companyHR.UserId;
        existingCompanyHR.CompanyId = companyHR.CompanyId;
        existingCompanyHR.Designation = companyHR.Designation;
        existingCompanyHR.ApprovedBy = companyHR.ApprovedBy;
        existingCompanyHR.ApprovedAt = companyHR.ApprovedAt;

        await _context.SaveChangesAsync();

        return true;
    }
}