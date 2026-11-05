using Campus_Management.DTOs.Companies;

namespace Campus_Management.Services.Interfaces;

public interface ICompanyService
{
    Task<List<CompanyResponse>> GetAllAsync();

    Task<CompanyResponse?> GetByIdAsync(int id);

    Task<CompanyResponse> CreateAsync(CreateCompanyRequest request);

    Task<bool> UpdateAsync(int id, UpdateCompanyRequest request);

    Task<bool> DeleteAsync(int id);
}