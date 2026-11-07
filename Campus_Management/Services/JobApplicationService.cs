using Campus_Management.DTOs.Applications;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class JobApplicationService : IJobApplicationService
{
    private readonly IJobApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IJobTargetCollegeRepository _targetRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;
    private readonly IApplicationStatusHistoryRepository _historyRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobApplicationService(
        IJobApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IJobTargetCollegeRepository targetRepository,
        IStudentRepository studentRepository,
        ICompanyHRRepository companyHRRepository,
        ICompanyRepository companyRepository,
        ICollegeRepository collegeRepository,
        IPlacementOfficerRepository placementOfficerRepository,
        IApplicationStatusHistoryRepository historyRepository,
        UserManager<ApplicationUser> userManager)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _targetRepository = targetRepository;
        _studentRepository = studentRepository;
        _companyHRRepository = companyHRRepository;
        _companyRepository = companyRepository;
        _collegeRepository = collegeRepository;
        _placementOfficerRepository = placementOfficerRepository;
        _historyRepository = historyRepository;
        _userManager = userManager;
    }

    public async Task<JobApplicationResponse?> ApplyAsync(
        CreateJobApplicationRequest request,
        string studentUserId)
    {
        var student =
            await _studentRepository.GetByUserIdAsync(studentUserId);

        if (student == null || student.Status != StudentStatus.Approved)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(studentUserId);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var job = await _jobRepository.GetByIdAsync(request.JobId);

        if (job == null ||
            job.Status != JobStatus.Published ||
            job.ApplicationDeadline <= DateTime.UtcNow)
        {
            return null;
        }

        if (await _applicationRepository.HasStudentAppliedAsync(
                job.JobId,
                student.StudentId))
        {
            return null;
        }

        if (job.MinimumCGPA.HasValue &&
            student.CGPA < job.MinimumCGPA.Value)
        {
            return null;
        }

        if (job.MaximumBacklogs.HasValue &&
            student.ActiveBacklogs > job.MaximumBacklogs.Value)
        {
            return null;
        }

        if (job.EligibleDepartments.Count > 0 &&
            !job.EligibleDepartments.Any(d =>
                string.Equals(d.Department, student.Department, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        if (job.EligibleGraduationYears.Count > 0 &&
            !job.EligibleGraduationYears.Any(y =>
                y.GraduationYear == student.GraduationYear))
        {
            return null;
        }

        if (!job.IsOpenToAllColleges)
        {
            var targets =
                await _targetRepository.GetByJobIdAsync(job.JobId);

            if (!targets.Any(t => t.CollegeId == student.CollegeId))
            {
                return null;
            }
        }

        var application = new JobApplication
        {
            JobId = job.JobId,
            StudentId = student.StudentId,
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Applied,
            Remarks = request.Remarks
        };

        await _applicationRepository.AddAsync(application);

        await _historyRepository.AddAsync(new ApplicationStatusHistory
        {
            ApplicationId = application.ApplicationId,
            FromStatus = null,
            ToStatus = ApplicationStatus.Applied,
            ChangedByUserId = studentUserId,
            ChangedAt = DateTime.UtcNow,
            Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? "Application submitted." : request.Remarks
        });

        return await BuildResponseAsync(application);
    }

    public async Task<List<JobApplicationResponse>> GetMyApplicationsAsync(
        string studentUserId)
    {
        var student =
            await _studentRepository.GetByUserIdAsync(studentUserId);

        if (student == null)
        {
            return [];
        }

        var applications =
            await _applicationRepository.GetByStudentIdAsync(
                student.StudentId);

        var responses = new List<JobApplicationResponse>();

        foreach (var app in applications)
        {
            responses.Add(await BuildResponseAsync(app));
        }

        return responses;
    }

    public async Task<List<JobApplicationResponse>> GetApplicationsByJobAsync(
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
        else if (role == "PlacementOfficer")
        {
            var user =
                await _userManager.FindByIdAsync(currentUserId);

            var officer =
                await _placementOfficerRepository.GetByEmailAsync(
                    user?.Email ?? string.Empty);

            if (officer == null)
            {
                return [];
            }

            var allApplications =
                await _applicationRepository.GetByJobIdAsync(jobId);

            var filtered = new List<JobApplicationResponse>();

            foreach (var app in allApplications)
            {
                var student =
                    await _studentRepository.GetByIdAsync(app.StudentId);

                if (student != null &&
                    student.CollegeId == officer.CollegeId)
                {
                    filtered.Add(await BuildResponseAsync(app));
                }
            }

            return filtered;
        }
        else if (role != "Admin")
        {
            return [];
        }

        var applications =
            await _applicationRepository.GetByJobIdAsync(jobId);

        var responses = new List<JobApplicationResponse>();

        foreach (var app in applications)
        {
            responses.Add(await BuildResponseAsync(app));
        }

        return responses;
    }

    public async Task<JobApplicationResponse?> GetByIdAsync(
        int id,
        string currentUserId,
        string role)
    {
        var application =
            await _applicationRepository.GetByIdAsync(id);

        if (application == null)
        {
            return null;
        }

        var job = await _jobRepository.GetByIdAsync(application.JobId);

        if (job == null)
        {
            return null;
        }

        if (role == "Student")
        {
            var student =
                await _studentRepository.GetByUserIdAsync(currentUserId);

            if (student == null ||
                student.StudentId != application.StudentId)
            {
                return null;
            }
        }
        else if (role == "CompanyHR")
        {
            var companyHR =
                await _companyHRRepository.GetByUserIdAsync(currentUserId);

            if (companyHR == null ||
                companyHR.CompanyId != job.CompanyId)
            {
                return null;
            }
        }
        else if (role == "PlacementOfficer")
        {
            var user =
                await _userManager.FindByIdAsync(currentUserId);

            var officer =
                await _placementOfficerRepository.GetByEmailAsync(
                    user?.Email ?? string.Empty);

            var student =
                await _studentRepository.GetByIdAsync(application.StudentId);

            if (officer == null ||
                student == null ||
                student.CollegeId != officer.CollegeId)
            {
                return null;
            }
        }
        else if (role != "Admin")
        {
            return null;
        }

        return await BuildResponseAsync(application);
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        UpdateApplicationStatusRequest request,
        string currentUserId,
        string role)
    {
        var application =
            await _applicationRepository.GetByIdAsync(id);

        if (application == null ||
            application.Status == ApplicationStatus.Withdrawn)
        {
            return false;
        }

        var job = await _jobRepository.GetByIdAsync(application.JobId);

        if (job == null)
        {
            return false;
        }

        if (role == "CompanyHR")
        {
            var companyHR =
                await _companyHRRepository.GetByUserIdAsync(currentUserId);

            if (companyHR == null ||
                companyHR.CompanyId != job.CompanyId)
            {
                return false;
            }
        }
        else if (role != "Admin")
        {
            return false;
        }

        var oldStatus = application.Status;
        application.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.Remarks))
        {
            application.Remarks = request.Remarks;
        }

        await _applicationRepository.UpdateAsync(application);

        await _historyRepository.AddAsync(new ApplicationStatusHistory
        {
            ApplicationId = application.ApplicationId,
            FromStatus = oldStatus,
            ToStatus = request.Status,
            ChangedByUserId = currentUserId,
            ChangedAt = DateTime.UtcNow,
            Remarks = request.Remarks
        });

        return true;
    }

    public async Task<bool> WithdrawAsync(
        int id,
        string studentUserId)
    {
        var application =
            await _applicationRepository.GetByIdAsync(id);

        if (application == null)
        {
            return false;
        }

        var student =
            await _studentRepository.GetByUserIdAsync(studentUserId);

        if (student == null ||
            application.StudentId != student.StudentId)
        {
            return false;
        }

        if (application.Status == ApplicationStatus.Selected ||
            application.Status == ApplicationStatus.Rejected ||
            application.Status == ApplicationStatus.Withdrawn)
        {
            return false;
        }

        var oldStatus = application.Status;
        application.Status = ApplicationStatus.Withdrawn;

        await _applicationRepository.UpdateAsync(application);

        await _historyRepository.AddAsync(new ApplicationStatusHistory
        {
            ApplicationId = application.ApplicationId,
            FromStatus = oldStatus,
            ToStatus = ApplicationStatus.Withdrawn,
            ChangedByUserId = studentUserId,
            ChangedAt = DateTime.UtcNow,
            Remarks = "Application withdrawn by candidate."
        });

        return true;
    }

    private async Task<JobApplicationResponse> BuildResponseAsync(
        JobApplication application)
    {
        var job = await _jobRepository.GetByIdAsync(application.JobId);

        var company = job != null
            ? await _companyRepository.GetByIdAsync(job.CompanyId)
            : null;

        var student =
            await _studentRepository.GetByIdAsync(application.StudentId);

        var user = student != null
            ? await _userManager.FindByIdAsync(student.UserId)
            : null;

        var college = student != null
            ? await _collegeRepository.GetByIdAsync(student.CollegeId)
            : null;

        return new JobApplicationResponse
        {
            ApplicationId = application.ApplicationId,
            JobId = application.JobId,
            JobTitle = job?.JobTitle ?? string.Empty,
            CompanyId = job?.CompanyId ?? 0,
            CompanyName = company?.CompanyName ?? string.Empty,

            StudentId = application.StudentId,
            StudentName = user?.FullName ?? string.Empty,
            StudentEmail = user?.Email ?? string.Empty,
            RegisterNumber = student?.RegisterNumber ?? string.Empty,
            Department = student?.Department ?? string.Empty,
            GraduationYear = student?.GraduationYear ?? 0,
            CGPA = student?.CGPA ?? 0,
            CollegeId = student?.CollegeId ?? 0,
            CollegeName = college?.CollegeName ?? string.Empty,
            ResumeUrl = student?.ResumeUrl,

            AppliedAt = application.AppliedAt,
            Status = application.Status,
            Remarks = application.Remarks
        };
    }
}
