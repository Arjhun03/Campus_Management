using Campus_Management.DTOs.Applications;

namespace Campus_Management.Services.Interfaces;

public interface IJobApplicationService
{
    Task<JobApplicationResponse?> ApplyAsync(
        CreateJobApplicationRequest request,
        string studentUserId);

    Task<List<JobApplicationResponse>> GetMyApplicationsAsync(
        string studentUserId);

    Task<List<JobApplicationResponse>> GetApplicationsByJobAsync(
        int jobId,
        string currentUserId,
        string role);

    Task<JobApplicationResponse?> GetByIdAsync(
        int id,
        string currentUserId,
        string role);

    Task<bool> UpdateStatusAsync(
        int id,
        UpdateApplicationStatusRequest request,
        string currentUserId,
        string role);

    Task<bool> WithdrawAsync(
        int id,
        string studentUserId);
}
