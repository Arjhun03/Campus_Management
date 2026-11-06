using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IJobTargetCollegeRepository
{
    Task<List<JobTargetCollege>> GetByJobIdAsync(int jobId);

    Task<JobTargetCollege?> GetAsync(
        int jobId,
        int collegeId);

    Task<bool> ExistsAsync(
        int jobId,
        int collegeId);

    Task<JobTargetCollege> AddAsync(
        JobTargetCollege target);

    Task DeleteAsync(
        JobTargetCollege target);
}