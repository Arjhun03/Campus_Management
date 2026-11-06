using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IInterviewScheduleRepository
{
    Task<InterviewSchedule?> GetByIdAsync(int id);
    Task<InterviewSchedule?> GetByRoundAndApplicationAsync(int roundId, int applicationId);
    Task<List<InterviewSchedule>> GetByRoundIdAsync(int roundId);
    Task<List<InterviewSchedule>> GetByApplicationIdAsync(int applicationId);
    Task<List<InterviewSchedule>> GetByStudentIdAsync(int studentId);
    Task<InterviewSchedule> AddAsync(InterviewSchedule schedule);
    Task UpdateAsync(InterviewSchedule schedule);
    Task DeleteAsync(InterviewSchedule schedule);
}
