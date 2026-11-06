using Campus_Management.DTOs.InterviewRounds;

namespace Campus_Management.Services.Interfaces;

public interface IInterviewRoundService
{
    Task<InterviewRoundResponse?> CreateAsync(CreateInterviewRoundRequest request, string userId);
    Task<InterviewRoundResponse?> GetByIdAsync(int id, string currentUserId, string role);
    Task<List<InterviewRoundResponse>> GetByDriveIdAsync(int driveId, string currentUserId, string role);
    Task<bool> UpdateAsync(int id, UpdateInterviewRoundRequest request, string userId);
    Task<bool> DeleteAsync(int id, string userId);
}
