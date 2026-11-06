using Campus_Management.DTOs.Jobs;

namespace Campus_Management.Services.Interfaces;

public interface IJobService
{
    Task<JobResponse?> CreateAsync(
        CreateJobRequest request,
        string userId);

    Task<List<JobResponse>> GetAllAsync(
        string userId,
        string role);

    Task<JobResponse?> GetByIdAsync(
        int id,
        string userId,
        string role);

    Task<bool> UpdateAsync(
        int id,
        UpdateJobRequest request,
        string userId);

    Task<bool> PublishAsync(
        int id,
        string userId);

    Task<bool> CloseAsync(
        int id,
        string userId);

    Task<bool> DeleteAsync(
        int id,
        string userId);

    Task<List<JobResponse>> GetEligibleAsync(
        string studentUserId);

    Task<JobEligibilityResponse?> GetEligibilityAsync(
        int jobId,
        string studentUserId);
}