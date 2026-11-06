using Campus_Management.DTOs.Jobs;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobTargetCollegeRepository _targetRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobService(
        IJobRepository jobRepository,
        IJobTargetCollegeRepository targetRepository,
        ICompanyHRRepository companyHRRepository,
        ICompanyRepository companyRepository,
        IStudentRepository studentRepository,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepository = jobRepository;
        _targetRepository = targetRepository;
        _companyHRRepository = companyHRRepository;
        _companyRepository = companyRepository;
        _studentRepository = studentRepository;
        _userManager = userManager;
    }

    public async Task<JobResponse?> CreateAsync(
        CreateJobRequest request,
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

        var company =
            await _companyRepository.GetByIdAsync(companyHR.CompanyId);

        if (company == null ||
            company.Status != CompanyStatus.Active)
        {
            return null;
        }

        if (request.ApplicationDeadline <= DateTime.UtcNow)
        {
            return null;
        }

        var job = new JobOpening
        {
            CompanyId = companyHR.CompanyId,
            CreatedByHRId = companyHR.CompanyHRId,
            JobTitle = request.JobTitle,
            JobDescription = request.JobDescription,
            JobType = request.JobType,
            Location = request.Location,
            SalaryPackage = request.SalaryPackage,
            MinimumCGPA = request.MinimumCGPA,
            MaximumBacklogs = request.MaximumBacklogs,
            ApplicationDeadline = request.ApplicationDeadline,
            IsOpenToAllColleges = request.IsOpenToAllColleges,
            Status = JobStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _jobRepository.AddAsync(job);

        return await BuildResponseAsync(job);
    }

    public async Task<List<JobResponse>> GetAllAsync(
        string userId,
        string role)
    {
        List<JobOpening> jobs;

        if (role == "Admin")
        {
            jobs = await _jobRepository.GetAllAsync();
        }
        else if (role == "CompanyHR")
        {
            var companyHR =
                await _companyHRRepository.GetByUserIdAsync(userId);

            if (companyHR == null)
            {
                return [];
            }

            jobs = await _jobRepository
                .GetByCompanyIdAsync(companyHR.CompanyId);
        }
        else
        {
            jobs = (await _jobRepository.GetAllAsync())
                .Where(j => j.Status == JobStatus.Published)
                .ToList();
        }

        var response = new List<JobResponse>();

        foreach (var job in jobs)
        {
            response.Add(await BuildResponseAsync(job));
        }

        return response;
    }

    public async Task<JobResponse?> GetByIdAsync(
        int id,
        string userId,
        string role)
    {
        var job = await _jobRepository.GetByIdAsync(id);

        if (job == null)
        {
            return null;
        }

        if (role == "Admin")
        {
            return await BuildResponseAsync(job);
        }

        if (role == "CompanyHR")
        {
            var companyHR =
                await _companyHRRepository.GetByUserIdAsync(userId);

            if (companyHR == null ||
                companyHR.CompanyId != job.CompanyId)
            {
                return null;
            }

            return await BuildResponseAsync(job);
        }

        if (job.Status != JobStatus.Published)
        {
            return null;
        }

        return await BuildResponseAsync(job);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateJobRequest request,
        string userId)
    {
        var job = await _jobRepository.GetByIdAsync(id);

        if (job == null ||
            job.Status != JobStatus.Draft)
        {
            return false;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyHRId != job.CreatedByHRId ||
            companyHR.CompanyId != job.CompanyId)
        {
            return false;
        }

        if (request.ApplicationDeadline <= DateTime.UtcNow)
        {
            return false;
        }

        job.JobTitle = request.JobTitle;
        job.JobDescription = request.JobDescription;
        job.JobType = request.JobType;
        job.Location = request.Location;
        job.SalaryPackage = request.SalaryPackage;
        job.MinimumCGPA = request.MinimumCGPA;
        job.MaximumBacklogs = request.MaximumBacklogs;
        job.ApplicationDeadline = request.ApplicationDeadline;
        job.IsOpenToAllColleges = request.IsOpenToAllColleges;

        await _jobRepository.UpdateAsync(job);

        return true;
    }

    public async Task<bool> PublishAsync(
        int id,
        string userId)
    {
        var job = await _jobRepository.GetByIdAsync(id);

        if (job == null ||
            job.Status != JobStatus.Draft)
        {
            return false;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyHRId != job.CreatedByHRId ||
            companyHR.CompanyId != job.CompanyId)
        {
            return false;
        }

        var company =
            await _companyRepository.GetByIdAsync(job.CompanyId);

        if (company == null ||
            company.Status != CompanyStatus.Active)
        {
            return false;
        }

        if (job.ApplicationDeadline <= DateTime.UtcNow)
        {
            return false;
        }

        if (!job.IsOpenToAllColleges)
        {
            var targets =
                await _targetRepository.GetByJobIdAsync(job.JobId);

            if (targets.Count == 0)
            {
                return false;
            }
        }

        job.Status = JobStatus.Published;

        await _jobRepository.UpdateAsync(job);

        return true;
    }

    public async Task<bool> CloseAsync(
        int id,
        string userId)
    {
        var job = await _jobRepository.GetByIdAsync(id);

        if (job == null ||
            job.Status != JobStatus.Published)
        {
            return false;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyHRId != job.CreatedByHRId ||
            companyHR.CompanyId != job.CompanyId)
        {
            return false;
        }

        job.Status = JobStatus.Closed;

        await _jobRepository.UpdateAsync(job);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var job = await _jobRepository.GetByIdAsync(id);

        if (job == null ||
            job.Status != JobStatus.Draft)
        {
            return false;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyHRId != job.CreatedByHRId ||
            companyHR.CompanyId != job.CompanyId)
        {
            return false;
        }

        await _jobRepository.DeleteAsync(job);

        return true;
    }

    public async Task<List<JobResponse>> GetEligibleAsync(
        string studentUserId)
    {
        var student =
            await _studentRepository.GetByUserIdAsync(studentUserId);

        if (student == null ||
            student.Status != StudentStatus.Approved)
        {
            return [];
        }

        var jobs = (await _jobRepository.GetAllAsync())
            .Where(j => j.Status == JobStatus.Published)
            .Where(j => j.ApplicationDeadline > DateTime.UtcNow)
            .ToList();

        var eligibleJobs = new List<JobResponse>();

        foreach (var job in jobs)
        {
            var eligibility =
                await CheckEligibilityAsync(job, student);

            if (eligibility.IsEligible)
            {
                eligibleJobs.Add(
                    await BuildResponseAsync(job));
            }
        }

        return eligibleJobs;
    }

    public async Task<JobEligibilityResponse?> GetEligibilityAsync(
        int jobId,
        string studentUserId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null)
        {
            return null;
        }

        var student =
            await _studentRepository.GetByUserIdAsync(studentUserId);

        if (student == null)
        {
            return new JobEligibilityResponse
            {
                JobId = jobId,
                IsEligible = false,
                Reason = "Student profile was not found."
            };
        }

        return await CheckEligibilityAsync(job, student);
    }

    private async Task<JobEligibilityResponse> CheckEligibilityAsync(
        JobOpening job,
        Student student)
    {
        if (student.Status != StudentStatus.Approved)
        {
            return Ineligible(
                job.JobId,
                "Student account is not approved.");
        }

        if (job.Status != JobStatus.Published)
        {
            return Ineligible(
                job.JobId,
                "Job is not currently published.");
        }

        if (job.ApplicationDeadline <= DateTime.UtcNow)
        {
            return Ineligible(
                job.JobId,
                "Application deadline has passed.");
        }

        if (job.MinimumCGPA.HasValue &&
            student.CGPA < job.MinimumCGPA.Value)
        {
            return Ineligible(
                job.JobId,
                "Student CGPA does not meet the minimum requirement.");
        }

        if (job.MaximumBacklogs.HasValue &&
            student.ActiveBacklogs > job.MaximumBacklogs.Value)
        {
            return Ineligible(
                job.JobId,
                "Student has more active backlogs than allowed.");
        }

        if (!job.IsOpenToAllColleges)
        {
            var targets =
                await _targetRepository.GetByJobIdAsync(job.JobId);

            if (!targets.Any(x => x.CollegeId == student.CollegeId))
            {
                return Ineligible(
                    job.JobId,
                    "Job is not targeted to the student's college.");
            }
        }

        return new JobEligibilityResponse
        {
            JobId = job.JobId,
            IsEligible = true,
            Reason = "Student meets the current job eligibility criteria."
        };
    }

    private static JobEligibilityResponse Ineligible(
        int jobId,
        string reason)
    {
        return new JobEligibilityResponse
        {
            JobId = jobId,
            IsEligible = false,
            Reason = reason
        };
    }

    private async Task<JobResponse> BuildResponseAsync(
        JobOpening job)
    {
        var company =
            await _companyRepository.GetByIdAsync(job.CompanyId);

        var companyHR =
            await _companyHRRepository.GetByIdAsync(
                job.CreatedByHRId);

        return new JobResponse
        {
            JobId = job.JobId,
            CompanyId = job.CompanyId,
            CompanyName = company?.CompanyName ?? string.Empty,

            CreatedByHRId = job.CreatedByHRId,
            CreatedByHRName = companyHR?.UserId ?? string.Empty,

            JobTitle = job.JobTitle,
            JobDescription = job.JobDescription,
            JobType = job.JobType,
            Location = job.Location,
            SalaryPackage = job.SalaryPackage,
            MinimumCGPA = job.MinimumCGPA,
            MaximumBacklogs = job.MaximumBacklogs,
            ApplicationDeadline = job.ApplicationDeadline,
            IsOpenToAllColleges = job.IsOpenToAllColleges,
            Status = job.Status,
            CreatedAt = job.CreatedAt
        };
    }
}