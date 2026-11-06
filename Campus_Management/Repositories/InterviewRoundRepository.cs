using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class InterviewRoundRepository : IInterviewRoundRepository
{
    private readonly ApplicationDbContext _context;

    public InterviewRoundRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InterviewRound?> GetByIdAsync(int id)
    {
        return await _context.InterviewRounds
            .FirstOrDefaultAsync(r => r.RoundId == id);
    }

    public async Task<List<InterviewRound>> GetByDriveIdAsync(int driveId)
    {
        return await _context.InterviewRounds
            .Where(r => r.DriveId == driveId)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();
    }

    public async Task<InterviewRound?> GetByDriveAndRoundNumberAsync(int driveId, int roundNumber)
    {
        return await _context.InterviewRounds
            .FirstOrDefaultAsync(r => r.DriveId == driveId && r.RoundNumber == roundNumber);
    }

    public async Task<InterviewRound> AddAsync(InterviewRound round)
    {
        await _context.InterviewRounds.AddAsync(round);
        await _context.SaveChangesAsync();

        return round;
    }

    public async Task UpdateAsync(InterviewRound round)
    {
        _context.InterviewRounds.Update(round);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(InterviewRound round)
    {
        _context.InterviewRounds.Remove(round);
        await _context.SaveChangesAsync();
    }
}
