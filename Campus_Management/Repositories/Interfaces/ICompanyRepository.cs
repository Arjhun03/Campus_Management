using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<List<Company>> GetAllAsync();

    Task<Company?> GetByIdAsync(int id);

    Task<Company> CreateAsync(Company company);

    Task<bool> UpdateAsync(Company company);

    Task<bool> DeleteAsync(int id);
}