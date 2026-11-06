using Campus_Management.DTOs.Students;

namespace Campus_Management.Services.Interfaces;

public interface IStudentService
{
    Task<StudentResponse?> EnrollAsync(
        EnrollStudentRequest request,
        string placementOfficerUserId);

    Task<List<StudentResponse>> BulkEnrollAsync(
        List<EnrollStudentRequest> requests,
        string placementOfficerUserId);

    Task<List<StudentResponse>> GetAllAsync();

    Task<List<StudentResponse>> GetPendingAsync();

    Task<StudentResponse?> GetByIdAsync(int id);

    Task<StudentResponse?> GetByUserIdAsync(string userId);

    Task<bool> ApproveAsync(int id, string placementOfficerUserId);

    Task<bool> RejectAsync(int id, string placementOfficerUserId);

    Task<bool> SuspendAsync(int id);

    Task<bool> UpdateAsync(
        int id,
        UpdateStudentRequest request,
        string currentUserId,
        bool isAdmin);
}