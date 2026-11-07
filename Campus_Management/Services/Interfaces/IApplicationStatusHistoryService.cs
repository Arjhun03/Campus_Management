using Campus_Management.DTOs.ApplicationStatusHistories;
using Campus_Management.Models.Enums;

namespace Campus_Management.Services.Interfaces;

public interface IApplicationStatusHistoryService
{
    Task<List<ApplicationStatusHistoryResponse>> GetByApplicationIdAsync(
        int applicationId,
        string currentUserId,
        string role);

    Task<ApplicationStatusHistoryResponse> RecordTransitionAsync(
        int applicationId,
        ApplicationStatus? fromStatus,
        ApplicationStatus toStatus,
        string changedByUserId,
        string? remarks);
}
