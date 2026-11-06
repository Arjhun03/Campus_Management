using Campus_Management.DTOs.Jobs;

namespace Campus_Management.Services.Interfaces;

public interface IJobTargetCollegeService
{
    Task<JobTargetCollegeResponse?> AddTargetAsync(
        int jobId,
        TargetCollegeRequest request,
        string userId);

    Task<List<JobTargetCollegeResponse>> GetTargetsAsync(
        int jobId,
        string userId);

    Task<bool> RemoveTargetAsync(
        int jobId,
        int collegeId,
        string userId);
}