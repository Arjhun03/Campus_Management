using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class InterviewScheduleRepository : IInterviewScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public InterviewScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InterviewSchedule?> GetByIdAsync(int id)
    {
        return await _context.InterviewSchedules
            .FirstOrDefaultAsync(s => s.ScheduleId == id);
    }

    public async Task<InterviewSchedule?> GetByRoundAndApplicationAsync(int roundId, int applicationId)
    {
        return await _context.InterviewSchedules
            .FirstOrDefaultAsync(s => s.RoundId == roundId && s.ApplicationId == applicationId);
    }

    public async Task<List<InterviewSchedule>> GetByRoundIdAsync(int roundId)
    {
        return await _context.InterviewSchedules
            .Where(s => s.RoundId == roundId)
            .OrderBy(s => s.ScheduledStartTime)
            .ToListAsync();
    }

    public async Task<List<InterviewSchedule>> GetByApplicationIdAsync(int applicationId)
    {
        return await _context.InterviewSchedules
            .Where(s => s.ApplicationId == applicationId)
            .OrderBy(s => s.ScheduledStartTime)
            .ToListAsync();
    }

    public async Task<List<InterviewSchedule>> GetByStudentIdAsync(int studentId)
    {
        return await (from s in _context.InterviewSchedules
                      join a in _context.JobApplications on s.ApplicationId equals a.ApplicationId
                      where a.StudentId == studentId
                      orderby s.ScheduledStartTime
                      select s).ToListAsync();
    }

    public async Task<InterviewSchedule> AddAsync(InterviewSchedule schedule)
    {
        await _context.InterviewSchedules.AddAsync(schedule);
        await _context.SaveChangesAsync();

        return schedule;
    }

    public async Task UpdateAsync(InterviewSchedule schedule)
    {
        _context.InterviewSchedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(InterviewSchedule schedule)
    {
        _context.InterviewSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }
}
