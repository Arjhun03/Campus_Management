using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Company>> GetAllAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyId == id);
    }

    public async Task<Company> CreateAsync(Company company)
    {
        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        return company;
    }

    public async Task<bool> UpdateAsync(Company company)
    {
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == company.CompanyId);

        if (existingCompany == null)
        {
            return false;
        }

        existingCompany.CompanyName = company.CompanyName;
        existingCompany.CompanyEmail = company.CompanyEmail;
        existingCompany.Website = company.Website;
        existingCompany.Industry = company.Industry;
        existingCompany.Address = company.Address;
        existingCompany.Description = company.Description;
        existingCompany.Status = company.Status;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == id);

        if (company == null)
        {
            return false;
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return true;
    }
}