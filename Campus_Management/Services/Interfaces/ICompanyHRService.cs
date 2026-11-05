using Campus_Management.DTOs.CompanyHRs;

namespace Campus_Management.Services.Interfaces;

public interface ICompanyHRService
{
    Task<CompanyHRResponse?> RegisterAsync(
        RegisterCompanyHRRequest request);

    Task<List<CompanyHRResponse>> GetAllAsync();

    Task<List<CompanyHRResponse>> GetPendingAsync();

    Task<CompanyHRResponse?> GetByIdAsync(int id);

    Task<CompanyHRResponse?> GetByUserIdAsync(string userId);

    Task<bool> ApproveAsync(
        int id,
        string approvedByUserId);

    Task<bool> RejectAsync(int id);
}