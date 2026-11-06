using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public JobApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?> GetByIdAsync(int id)
    {
        return await _context.JobApplications
            .FirstOrDefaultAsync(a => a.ApplicationId == id);
    }

    public async Task<JobApplication?> GetByJobAndStudentAsync(
        int jobId,
        int studentId)
    {
        return await _context.JobApplications
            .FirstOrDefaultAsync(a =>
                a.JobId == jobId &&
                a.StudentId == studentId);
    }

    public async Task<bool> HasStudentAppliedAsync(
        int jobId,
        int studentId)
    {
        return await _context.JobApplications
            .AnyAsync(a =>
                a.JobId == jobId &&
                a.StudentId == studentId);
    }

    public async Task<List<JobApplication>> GetByStudentIdAsync(int studentId)
    {
        return await _context.JobApplications
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetByJobIdAsync(int jobId)
    {
        return await _context.JobApplications
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetAllAsync()
    {
        return await _context.JobApplications
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<JobApplication> AddAsync(JobApplication application)
    {
        await _context.JobApplications.AddAsync(application);
        await _context.SaveChangesAsync();

        return application;
    }

    public async Task UpdateAsync(JobApplication application)
    {
        _context.JobApplications.Update(application);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(JobApplication application)
    {
        _context.JobApplications.Remove(application);
        await _context.SaveChangesAsync();
    }
}
