using Campus_Management.DTOs.Colleges;

namespace Campus_Management.Services.Interfaces;

public interface ICollegeService
{
    Task<IEnumerable<CollegeResponse>> GetAllAsync();

    Task<CollegeResponse?> GetByIdAsync(int collegeId);

    Task<CollegeResponse> CreateAsync(CreateCollegeRequest request);

    Task<bool> UpdateAsync(int collegeId, UpdateCollegeRequest request);

    Task<bool> DeleteAsync(int collegeId);
}