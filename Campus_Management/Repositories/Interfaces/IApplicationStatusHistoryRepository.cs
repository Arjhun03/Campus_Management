using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IApplicationStatusHistoryRepository
{
    Task<ApplicationStatusHistory?> GetByIdAsync(int id);
    Task<List<ApplicationStatusHistory>> GetByApplicationIdAsync(int applicationId);
    Task<ApplicationStatusHistory> AddAsync(ApplicationStatusHistory history);
}
