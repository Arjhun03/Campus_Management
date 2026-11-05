using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface ICompanyHRRepository
{
    Task<List<CompanyHR>> GetAllAsync();

    Task<List<CompanyHR>> GetPendingAsync();

    Task<CompanyHR?> GetByIdAsync(int id);

    Task<CompanyHR?> GetByUserIdAsync(string userId);

    Task<CompanyHR> CreateAsync(CompanyHR companyHR);

    Task<bool> UpdateAsync(CompanyHR companyHR);
}