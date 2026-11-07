using Campus_Management.DTOs.RecruitmentDrives;

namespace Campus_Management.Services.Interfaces;

public interface IRecruitmentDriveService
{
    Task<RecruitmentDriveResponse?> CreateAsync(
        CreateRecruitmentDriveRequest request,
        string userId);

    Task<RecruitmentDriveResponse?> GetByIdAsync(
        int id,
        string currentUserId,
        string role);

    Task<List<RecruitmentDriveResponse>> GetByJobIdAsync(
        int jobId,
        string currentUserId,
        string role);

    Task<List<RecruitmentDriveResponse>> GetAllAsync(
        string currentUserId,
        string role);

    Task<bool> UpdateAsync(
        int id,
        UpdateRecruitmentDriveRequest request,
        string userId);

    Task<bool> CancelAsync(
        int id,
        string userId);

    Task<bool> DeleteAsync(
        int id,
        string userId);
}
