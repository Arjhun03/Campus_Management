using Campus_Management.DTOs.RecruitmentDrives;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class RecruitmentDriveService : IRecruitmentDriveService
{
    private readonly IRecruitmentDriveRepository _driveRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobTargetCollegeRepository _targetRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public RecruitmentDriveService(
        IRecruitmentDriveRepository driveRepository,
        IJobRepository jobRepository,
        ICompanyHRRepository companyHRRepository,
        ICompanyRepository companyRepository,
        IJobTargetCollegeRepository targetRepository,
        IStudentRepository studentRepository,
        IPlacementOfficerRepository placementOfficerRepository,
        UserManager<ApplicationUser> userManager)
    {
        _driveRepository = driveRepository;
        _jobRepository = jobRepository;
        _companyHRRepository = companyHRRepository;
        _companyRepository = companyRepository;
        _targetRepository = targetRepository;
        _studentRepository = studentRepository;
        _placementOfficerRepository = placementOfficerRepository;
        _userManager = userManager;
    }

    public async Task<RecruitmentDriveResponse?> CreateAsync(
        CreateRecruitmentDriveRequest request,
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null || companyHR.ApprovedAt == null)
        {
            return null;
        }

        var job = await _jobRepository.GetByIdAsync(request.JobId);

        if (job == null || job.CompanyId != companyHR.CompanyId)
        {
            return null;
        }

        var company =
            await _companyRepository.GetByIdAsync(companyHR.CompanyId);

        if (company == null || company.Status != CompanyStatus.Active)
        {
            return null;
        }

        var drive = new RecruitmentDrive
        {
            JobId = request.JobId,
            CompanyId = companyHR.CompanyId,
            DriveName = request.DriveName,
            DriveDate = request.DriveDate,
            EndDate = request.EndDate,
            Venue = request.Venue,
            DriveMode = request.DriveMode,
            Status = DriveStatus.Scheduled,
            Instructions = request.Instructions,
            CreatedAt = DateTime.UtcNow
        };

        await _driveRepository.AddAsync(drive);

        return await BuildResponseAsync(drive);
    }

    public async Task<RecruitmentDriveResponse?> GetByIdAsync(
        int id,
        string currentUserId,
        string role)
    {
        var drive = await _driveRepository.GetByIdAsync(id);

        if (drive == null)
        {
            return null;
        }

        if (role == "CompanyHR")
        {
            var companyHR =
                await _companyHRRepository.GetByUserIdAsync(currentUserId);

            if (companyHR == null ||
                companyHR.CompanyId != drive.CompanyId)
            {
                return null;
            }
        }
        else if (role == "PlacementOfficer")
        {
            var user = await _userManager.FindByIdAsync(currentUserId);
            var officer = await _placementOfficerRepository.GetByEmailAsync(user?.Email ?? string.Empty);

            if (officer == null)
            {
                return null;
            }

            var job = await _jobRepository.GetByIdAsync(drive.JobId);

            if (job == null || (!job.IsOpenToAllColleges && !(await IsCollegeTargetedAsync(job.JobId, officer.CollegeId))))
            {
                return null;
            }
        }
        else if (role == "Student")
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);

            if (student == null)
            {
                return null;
            }

            var job = await _jobRepository.GetByIdAsync(drive.JobId);

            if (job == null || (!job.IsOpenToAllColleges && !(await IsCollegeTargetedAsync(job.JobId, student.CollegeId))))
            {
                return null;
            }
        }
        else if (role != "Admin" && role != "SuperAdmin")
        {
            return null;
        }

        return await BuildResponseAsync(drive);
    }

    public async Task<List<RecruitmentDriveResponse>> GetByJobIdAsync(
        int jobId,
        string currentUserId,
        string role)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null)
        {
            return [];
        }

        if (role == "CompanyHR")
        {
            var companyHR =
                await _companyHRRepository.GetByUserIdAsync(currentUserId);

            if (companyHR == null ||
                companyHR.CompanyId != job.CompanyId)
            {
                return [];
            }
        }
        else if (role != "Admin" && role != "SuperAdmin")
        {
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
                return [];
            }

            if (!job.IsOpenToAllColleges && !(await IsCollegeTargetedAsync(job.JobId, collegeId)))
            {
                return [];
            }
        }

        var drives = await _driveRepository.GetByJobIdAsync(jobId);
        var responses = new List<RecruitmentDriveResponse>();

        foreach (var drive in drives)
        {
            responses.Add(await BuildResponseAsync(drive));
        }

        return responses;
    }

    public async Task<List<RecruitmentDriveResponse>> GetAllAsync(
        string currentUserId,
        string role)
    {
        List<RecruitmentDrive> drives;

        if (role == "Admin" || role == "SuperAdmin")
        {
            drives = await _driveRepository.GetAllAsync();
        }
        else if (role == "CompanyHR")
        {
            var companyHR =
                await _companyHRRepository.GetByUserIdAsync(currentUserId);

            if (companyHR == null)
            {
                return [];
            }

            drives = await _driveRepository.GetByCompanyIdAsync(companyHR.CompanyId);
        }
        else
        {
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
                return [];
            }

            var allDrives = await _driveRepository.GetAllAsync();
            var filteredDrives = new List<RecruitmentDrive>();

            foreach (var drive in allDrives)
            {
                var job = await _jobRepository.GetByIdAsync(drive.JobId);

                if (job != null && (job.IsOpenToAllColleges || await IsCollegeTargetedAsync(job.JobId, collegeId)))
                {
                    filteredDrives.Add(drive);
                }
            }

            drives = filteredDrives;
        }

        var responses = new List<RecruitmentDriveResponse>();

        foreach (var drive in drives)
        {
            responses.Add(await BuildResponseAsync(drive));
        }

        return responses;
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateRecruitmentDriveRequest request,
        string userId)
    {
        var drive = await _driveRepository.GetByIdAsync(id);

        if (drive == null)
        {
            return false;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyId != drive.CompanyId)
        {
            return false;
        }

        drive.DriveName = request.DriveName;
        drive.DriveDate = request.DriveDate;
        drive.EndDate = request.EndDate;
        drive.Venue = request.Venue;
        drive.DriveMode = request.DriveMode;
        drive.Status = request.Status;
        drive.Instructions = request.Instructions;

        await _driveRepository.UpdateAsync(drive);

        return true;
    }

    public async Task<bool> CancelAsync(
        int id,
        string userId)
    {
        var drive = await _driveRepository.GetByIdAsync(id);

        if (drive == null)
        {
            return false;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyId != drive.CompanyId)
        {
            return false;
        }

        if (drive.Status == DriveStatus.Completed ||
            drive.Status == DriveStatus.Cancelled)
        {
            return false;
        }

        drive.Status = DriveStatus.Cancelled;

        await _driveRepository.UpdateAsync(drive);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var drive = await _driveRepository.GetByIdAsync(id);

        if (drive == null)
        {
            return false;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyId != drive.CompanyId)
        {
            return false;
        }

        if (drive.Status != DriveStatus.Scheduled &&
            drive.Status != DriveStatus.Cancelled)
        {
            return false;
        }

        await _driveRepository.DeleteAsync(drive);

        return true;
    }

    private async Task<bool> IsCollegeTargetedAsync(int jobId, int collegeId)
    {
        var targets = await _targetRepository.GetByJobIdAsync(jobId);
        return targets.Any(t => t.CollegeId == collegeId);
    }

    private async Task<RecruitmentDriveResponse> BuildResponseAsync(
        RecruitmentDrive drive)
    {
        var job = await _jobRepository.GetByIdAsync(drive.JobId);
        var company = await _companyRepository.GetByIdAsync(drive.CompanyId);

        return new RecruitmentDriveResponse
        {
            DriveId = drive.DriveId,
            JobId = drive.JobId,
            JobTitle = job?.JobTitle ?? string.Empty,
            CompanyId = drive.CompanyId,
            CompanyName = company?.CompanyName ?? string.Empty,
            DriveName = drive.DriveName,
            DriveDate = drive.DriveDate,
            EndDate = drive.EndDate,
            Venue = drive.Venue,
            DriveMode = drive.DriveMode,
            Status = drive.Status,
            Instructions = drive.Instructions,
            CreatedAt = drive.CreatedAt
        };
    }
}
