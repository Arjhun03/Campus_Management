using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IInterviewRoundRepository
{
    Task<InterviewRound?> GetByIdAsync(int id);
    Task<List<InterviewRound>> GetByDriveIdAsync(int driveId);
    Task<List<InterviewRound>> GetByJobIdAsync(int jobId);
    Task<InterviewRound?> GetByDriveAndRoundNumberAsync(int driveId, int roundNumber);
    Task<InterviewRound> AddAsync(InterviewRound round);
    Task UpdateAsync(InterviewRound round);
    Task DeleteAsync(InterviewRound round);
}
