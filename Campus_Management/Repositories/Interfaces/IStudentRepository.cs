using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id);

    Task<Student?> GetByUserIdAsync(string userId);

    Task<List<Student>> GetAllAsync();

    Task<List<Student>> GetPendingAsync();

    Task<bool> RegisterNumberExistsAsync(
        int collegeId,
        string registerNumber);

    Task<Student> AddAsync(Student student);

    Task UpdateAsync(Student student);

    Task DeleteAsync(Student student);
}