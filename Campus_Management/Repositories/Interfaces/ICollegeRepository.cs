using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface ICollegeRepository
{
    Task<IEnumerable<College>> GetAllAsync();

    Task<College?> GetByIdAsync(int collegeId);

    Task<College> AddAsync(College college);

    Task UpdateAsync(College college);

    Task DeleteAsync(College college);
}
