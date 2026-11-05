using Campus_Management.DTOs.Companies;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;

namespace Campus_Management.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<List<CompanyResponse>> GetAllAsync()
    {
        var companies = await _companyRepository.GetAllAsync();

        return companies.Select(MapToResponse).ToList();
    }

    public async Task<CompanyResponse?> GetByIdAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);

        return company == null
            ? null
            : MapToResponse(company);
    }

    public async Task<CompanyResponse> CreateAsync(
        CreateCompanyRequest request)
    {
        var company = new Company
        {
            CompanyName = request.CompanyName,
            CompanyEmail = request.CompanyEmail,
            Website = request.Website,
            Industry = request.Industry,
            Address = request.Address,
            Description = request.Description,
            Status = CompanyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var createdCompany =
            await _companyRepository.CreateAsync(company);

        return MapToResponse(createdCompany);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateCompanyRequest request)
    {
        var company = new Company
        {
            CompanyId = id,
            CompanyName = request.CompanyName,
            CompanyEmail = request.CompanyEmail,
            Website = request.Website,
            Industry = request.Industry,
            Address = request.Address,
            Description = request.Description,
            Status = request.Status
        };

        return await _companyRepository.UpdateAsync(company);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _companyRepository.DeleteAsync(id);
    }

    private static CompanyResponse MapToResponse(
        Company company)
    {
        return new CompanyResponse
        {
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName,
            CompanyEmail = company.CompanyEmail,
            Website = company.Website,
            Industry = company.Industry,
            Address = company.Address,
            Description = company.Description,
            Status = company.Status,
            CreatedAt = company.CreatedAt
        };
    }
}