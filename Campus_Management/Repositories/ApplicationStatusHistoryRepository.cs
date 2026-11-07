using Campus_Management.Data;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class ApplicationStatusHistoryRepository : IApplicationStatusHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationStatusHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationStatusHistory?> GetByIdAsync(int id)
    {
        return await _context.ApplicationStatusHistories
            .FirstOrDefaultAsync(h => h.HistoryId == id);
    }

    public async Task<List<ApplicationStatusHistory>> GetByApplicationIdAsync(int applicationId)
    {
        return await _context.ApplicationStatusHistories
            .Where(h => h.ApplicationId == applicationId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<ApplicationStatusHistory> AddAsync(ApplicationStatusHistory history)
    {
        await _context.ApplicationStatusHistories.AddAsync(history);
        await _context.SaveChangesAsync();

        return history;
    }
}
