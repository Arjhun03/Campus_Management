using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IRecruitmentDriveRepository
{
    Task<RecruitmentDrive?> GetByIdAsync(int id);

    Task<List<RecruitmentDrive>> GetByJobIdAsync(int jobId);

    Task<List<RecruitmentDrive>> GetByCompanyIdAsync(int companyId);

    Task<List<RecruitmentDrive>> GetAllAsync();

    Task<RecruitmentDrive> AddAsync(RecruitmentDrive drive);

    Task UpdateAsync(RecruitmentDrive drive);

    Task DeleteAsync(RecruitmentDrive drive);
}
