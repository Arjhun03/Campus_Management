using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(int id);

    Task<JobApplication?> GetByJobAndStudentAsync(int jobId, int studentId);

    Task<bool> HasStudentAppliedAsync(int jobId, int studentId);

    Task<List<JobApplication>> GetByStudentIdAsync(int studentId);

    Task<List<JobApplication>> GetByJobIdAsync(int jobId);

    Task<List<JobApplication>> GetAllAsync();

    Task<JobApplication> AddAsync(JobApplication application);

    Task UpdateAsync(JobApplication application);

    Task DeleteAsync(JobApplication application);
}
