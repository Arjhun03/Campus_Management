using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IJobRepository
{
    Task<JobOpening?> GetByIdAsync(int id);

    Task<List<JobOpening>> GetAllAsync();

    Task<List<JobOpening>> GetByCompanyIdAsync(int companyId);

    Task<JobOpening> AddAsync(JobOpening job);

    Task UpdateAsync(JobOpening job);

    Task DeleteAsync(JobOpening job);
}