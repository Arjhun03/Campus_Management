using Campus_Management.DTOs.InterviewSchedules;

namespace Campus_Management.Services.Interfaces;

public interface IInterviewScheduleService
{
    Task<InterviewScheduleResponse?> CreateAsync(CreateInterviewScheduleRequest request, string userId);
    Task<InterviewScheduleResponse?> GetByIdAsync(int id, string currentUserId, string role);
    Task<List<InterviewScheduleResponse>> GetByRoundIdAsync(int roundId, string currentUserId, string role);
    Task<List<InterviewScheduleResponse>> GetByApplicationIdAsync(int applicationId, string currentUserId, string role);
    Task<List<InterviewScheduleResponse>> GetMySchedulesAsync(string currentUserId);
    Task<bool> UpdateAsync(int id, UpdateInterviewScheduleRequest request, string userId);
    Task<bool> SubmitFeedbackAsync(int id, InterviewFeedbackRequest request, string userId);
    Task<bool> DeleteAsync(int id, string userId);
}
