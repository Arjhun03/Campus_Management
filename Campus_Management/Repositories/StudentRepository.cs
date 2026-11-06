using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == id);
    }

    public async Task<Student?> GetByUserIdAsync(string userId)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students
            .OrderBy(s => s.StudentId)
            .ToListAsync();
    }

    public async Task<List<Student>> GetPendingAsync()
    {
        return await _context.Students
            .Where(s => s.Status == StudentStatus.Pending)
            .OrderBy(s => s.StudentId)
            .ToListAsync();
    }

    public async Task<List<Student>> GetByCollegeIdAsync(int collegeId)
    {
        return await _context.Students
            .Where(s => s.CollegeId == collegeId)
            .OrderBy(s => s.StudentId)
            .ToListAsync();
    }

    public async Task<List<Student>> GetPendingByCollegeIdAsync(int collegeId)
    {
        return await _context.Students
            .Where(s => s.CollegeId == collegeId && s.Status == StudentStatus.Pending)
            .OrderBy(s => s.StudentId)
            .ToListAsync();
    }

    public async Task<bool> RegisterNumberExistsAsync(
        int collegeId,
        string registerNumber)
    {
        return await _context.Students
            .AnyAsync(s =>
                s.CollegeId == collegeId &&
                s.RegisterNumber == registerNumber);
    }

    public async Task<Student> AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        return student;
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Student student)
    {
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }
}
