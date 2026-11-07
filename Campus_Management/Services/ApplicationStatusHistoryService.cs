using Campus_Management.DTOs.ApplicationStatusHistories;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class ApplicationStatusHistoryService : IApplicationStatusHistoryService
{
    private readonly IApplicationStatusHistoryRepository _historyRepository;
    private readonly IJobApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationStatusHistoryService(
        IApplicationStatusHistoryRepository historyRepository,
        IJobApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IStudentRepository studentRepository,
        ICompanyHRRepository companyHRRepository,
        IPlacementOfficerRepository placementOfficerRepository,
        UserManager<ApplicationUser> userManager)
    {
        _historyRepository = historyRepository;
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _studentRepository = studentRepository;
        _companyHRRepository = companyHRRepository;
        _placementOfficerRepository = placementOfficerRepository;
        _userManager = userManager;
    }

    public async Task<List<ApplicationStatusHistoryResponse>> GetByApplicationIdAsync(
        int applicationId,
        string currentUserId,
        string role)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
        {
            return new List<ApplicationStatusHistoryResponse>();
        }

        var hasAccess = await HasAccessToApplicationAsync(application, currentUserId, role);
        if (!hasAccess)
        {
            return new List<ApplicationStatusHistoryResponse>();
        }

        var histories = await _historyRepository.GetByApplicationIdAsync(applicationId);
        var responses = new List<ApplicationStatusHistoryResponse>();

        foreach (var history in histories)
        {
            responses.Add(await BuildResponseAsync(history));
        }

        return responses;
    }

    public async Task<ApplicationStatusHistoryResponse> RecordTransitionAsync(
        int applicationId,
        ApplicationStatus? fromStatus,
        ApplicationStatus toStatus,
        string changedByUserId,
        string? remarks)
    {
        var history = new ApplicationStatusHistory
        {
            ApplicationId = applicationId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow,
            Remarks = remarks
        };

        await _historyRepository.AddAsync(history);

        return await BuildResponseAsync(history);
    }

    private async Task<bool> HasAccessToApplicationAsync(
        JobApplication application,
        string currentUserId,
        string role)
    {
        if (role == "Admin" || role == "SuperAdmin")
        {
            return true;
        }

        if (role == "CompanyHR")
        {
            var companyHR = await _companyHRRepository.GetByUserIdAsync(currentUserId);
            var job = await _jobRepository.GetByIdAsync(application.JobId);
            return companyHR != null && job != null && companyHR.CompanyId == job.CompanyId;
        }

        if (role == "Student")
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            return student != null && student.StudentId == application.StudentId;
        }

        if (role == "PlacementOfficer")
        {
            var user = await _userManager.FindByIdAsync(currentUserId);
            var officer = await _placementOfficerRepository.GetByEmailAsync(user?.Email ?? string.Empty);
            var student = await _studentRepository.GetByIdAsync(application.StudentId);
            return officer != null && student != null && student.CollegeId == officer.CollegeId;
        }

        return false;
    }

    private async Task<ApplicationStatusHistoryResponse> BuildResponseAsync(
        ApplicationStatusHistory history)
    {
        var user = await _userManager.FindByIdAsync(history.ChangedByUserId);
        var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

        return new ApplicationStatusHistoryResponse
        {
            HistoryId = history.HistoryId,
            ApplicationId = history.ApplicationId,
            FromStatus = history.FromStatus,
            FromStatusText = history.FromStatus?.ToString(),
            ToStatus = history.ToStatus,
            ToStatusText = history.ToStatus.ToString(),
            ChangedByUserId = history.ChangedByUserId,
            ChangedByName = user?.FullName ?? user?.UserName ?? "System",
            ChangedByRole = roles.FirstOrDefault() ?? "Unknown",
            ChangedAt = history.ChangedAt,
            Remarks = history.Remarks
        };
    }
}
