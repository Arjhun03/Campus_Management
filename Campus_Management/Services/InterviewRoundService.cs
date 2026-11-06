using Campus_Management.DTOs.InterviewRounds;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class InterviewRoundService : IInterviewRoundService
{
    private readonly IInterviewRoundRepository _roundRepository;
    private readonly IRecruitmentDriveRepository _driveRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobTargetCollegeRepository _targetRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public InterviewRoundService(
        IInterviewRoundRepository roundRepository,
        IRecruitmentDriveRepository driveRepository,
        IJobRepository jobRepository,
        ICompanyHRRepository companyHRRepository,
        ICompanyRepository companyRepository,
        IJobTargetCollegeRepository targetRepository,
        IStudentRepository studentRepository,
        IPlacementOfficerRepository placementOfficerRepository,
        UserManager<ApplicationUser> userManager)
    {
        _roundRepository = roundRepository;
        _driveRepository = driveRepository;
        _jobRepository = jobRepository;
        _companyHRRepository = companyHRRepository;
        _companyRepository = companyRepository;
        _targetRepository = targetRepository;
        _studentRepository = studentRepository;
        _placementOfficerRepository = placementOfficerRepository;
        _userManager = userManager;
    }

    public async Task<InterviewRoundResponse?> CreateAsync(
        CreateInterviewRoundRequest request,
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var companyHR = await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null || companyHR.ApprovedAt == null)
        {
            return null;
        }

        var company = await _companyRepository.GetByIdAsync(companyHR.CompanyId);

        if (company == null || company.Status != CompanyStatus.Active)
        {
            return null;
        }

        var drive = await _driveRepository.GetByIdAsync(request.DriveId);

        if (drive == null || drive.CompanyId != companyHR.CompanyId)
        {
            return null;
        }

        // Check if round number already exists for this drive
        var existingRound = await _roundRepository.GetByDriveAndRoundNumberAsync(request.DriveId, request.RoundNumber);

        if (existingRound != null)
        {
            return null;
        }

        var round = new InterviewRound
        {
            DriveId = request.DriveId,
            RoundNumber = request.RoundNumber,
            RoundName = request.RoundName,
            RoundType = request.RoundType,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _roundRepository.AddAsync(round);

        return await BuildResponseAsync(round, drive);
    }

    public async Task<InterviewRoundResponse?> GetByIdAsync(
        int id,
        string currentUserId,
        string role)
    {
        var round = await _roundRepository.GetByIdAsync(id);

        if (round == null)
        {
            return null;
        }

        var drive = await _driveRepository.GetByIdAsync(round.DriveId);

        if (drive == null)
        {
            return null;
        }

        var hasAccess = await HasAccessToDriveAsync(drive, currentUserId, role);

        if (!hasAccess)
        {
            return null;
        }

        return await BuildResponseAsync(round, drive);
    }

    public async Task<List<InterviewRoundResponse>> GetByDriveIdAsync(
        int driveId,
        string currentUserId,
        string role)
    {
        var drive = await _driveRepository.GetByIdAsync(driveId);

        if (drive == null)
        {
            return new List<InterviewRoundResponse>();
        }

        var hasAccess = await HasAccessToDriveAsync(drive, currentUserId, role);

        if (!hasAccess)
        {
            return new List<InterviewRoundResponse>();
        }

        var rounds = await _roundRepository.GetByDriveIdAsync(driveId);
        var responses = new List<InterviewRoundResponse>();

        foreach (var round in rounds)
        {
            var res = await BuildResponseAsync(round, drive);
            if (res != null)
            {
                responses.Add(res);
            }
        }

        return responses;
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateInterviewRoundRequest request,
        string userId)
    {
        var round = await _roundRepository.GetByIdAsync(id);

        if (round == null)
        {
            return false;
        }

        var drive = await _driveRepository.GetByIdAsync(round.DriveId);

        if (drive == null)
        {
            return false;
        }

        var companyHR = await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null || companyHR.ApprovedAt == null || companyHR.CompanyId != drive.CompanyId)
        {
            return false;
        }

        if (round.RoundNumber != request.RoundNumber)
        {
            var existingWithNumber = await _roundRepository.GetByDriveAndRoundNumberAsync(round.DriveId, request.RoundNumber);
            if (existingWithNumber != null && existingWithNumber.RoundId != id)
            {
                return false;
            }
        }

        round.RoundNumber = request.RoundNumber;
        round.RoundName = request.RoundName;
        round.RoundType = request.RoundType;
        round.Description = request.Description;

        await _roundRepository.UpdateAsync(round);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var round = await _roundRepository.GetByIdAsync(id);

        if (round == null)
        {
            return false;
        }

        var drive = await _driveRepository.GetByIdAsync(round.DriveId);

        if (drive == null)
        {
            return false;
        }

        var companyHR = await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null || companyHR.ApprovedAt == null || companyHR.CompanyId != drive.CompanyId)
        {
            return false;
        }

        await _roundRepository.DeleteAsync(round);

        return true;
    }

    private async Task<bool> HasAccessToDriveAsync(
        RecruitmentDrive drive,
        string currentUserId,
        string role)
    {
        if (role == "Admin" || role == "SuperAdmin")
        {
            return true;
        }

        if (role == "CompanyHR")
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
            return hr != null && hr.CompanyId == drive.CompanyId;
        }

        int collegeId = 0;

        if (role == "PlacementOfficer")
        {
            var user = await _userManager.FindByIdAsync(currentUserId);
            var officer = await _placementOfficerRepository.GetByEmailAsync(user?.Email ?? string.Empty);
            collegeId = officer?.CollegeId ?? 0;
        }
        else if (role == "Student")
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            collegeId = student?.CollegeId ?? 0;
        }

        if (collegeId == 0)
        {
            return false;
        }

        var job = await _jobRepository.GetByIdAsync(drive.JobId);
        if (job == null)
        {
            return false;
        }

        if (job.IsOpenToAllColleges)
        {
            return true;
        }

        var targets = await _targetRepository.GetByJobIdAsync(drive.JobId);
        return targets.Any(t => t.CollegeId == collegeId);
    }

    private async Task<InterviewRoundResponse?> BuildResponseAsync(
        InterviewRound round,
        RecruitmentDrive drive)
    {
        var job = await _jobRepository.GetByIdAsync(drive.JobId);
        var company = await _companyRepository.GetByIdAsync(drive.CompanyId);

        return new InterviewRoundResponse
        {
            RoundId = round.RoundId,
            DriveId = round.DriveId,
            DriveName = drive.DriveName,
            JobId = drive.JobId,
            JobTitle = job?.JobTitle ?? string.Empty,
            CompanyId = drive.CompanyId,
            CompanyName = company?.CompanyName ?? string.Empty,
            RoundNumber = round.RoundNumber,
            RoundName = round.RoundName,
            RoundType = round.RoundType,
            Description = round.Description,
            CreatedAt = round.CreatedAt
        };
    }
}
