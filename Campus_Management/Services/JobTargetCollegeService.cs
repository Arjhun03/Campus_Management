using Campus_Management.DTOs.Jobs;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class JobTargetCollegeService : IJobTargetCollegeService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobTargetCollegeRepository _targetRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobTargetCollegeService(
        IJobRepository jobRepository,
        IJobTargetCollegeRepository targetRepository,
        ICompanyHRRepository companyHRRepository,
        ICollegeRepository collegeRepository,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepository = jobRepository;
        _targetRepository = targetRepository;
        _companyHRRepository = companyHRRepository;
        _collegeRepository = collegeRepository;
        _userManager = userManager;
    }

    public async Task<JobTargetCollegeResponse?> AddTargetAsync(
        int jobId,
        TargetCollegeRequest request,
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null ||
            job.Status != Models.Enums.JobStatus.Draft)
        {
            return null;
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyHRId != job.CreatedByHRId ||
            companyHR.CompanyId != job.CompanyId)
        {
            return null;
        }

        var college =
            await _collegeRepository.GetByIdAsync(request.CollegeId);

        if (college == null)
        {
            return null;
        }

        var exists =
            await _targetRepository.ExistsAsync(
                jobId,
                request.CollegeId);

        if (exists)
        {
            return null;
        }

        var target = new JobTargetCollege
        {
            JobId = jobId,
            CollegeId = request.CollegeId
        };

        await _targetRepository.AddAsync(target);

        return new JobTargetCollegeResponse
        {
            JobTargetCollegeId = target.JobTargetCollegeId,
            JobId = target.JobId,
            CollegeId = target.CollegeId,
            CollegeName = college.CollegeName
        };
    }

    public async Task<List<JobTargetCollegeResponse>> GetTargetsAsync(
        int jobId,
        string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null)
        {
            return [];
        }

        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null ||
            companyHR.CompanyHRId != job.CreatedByHRId ||
            companyHR.CompanyId != job.CompanyId)
        {
            return [];
        }

        var targets =
            await _targetRepository.GetByJobIdAsync(jobId);

        var response =
            new List<JobTargetCollegeResponse>();

        foreach (var target in targets)
        {
            var college =
                await _collegeRepository.GetByIdAsync(
                    target.CollegeId);

            if (college == null)
            {
                continue;
            }

            response.Add(new JobTargetCollegeResponse
            {
                JobTargetCollegeId =
                    target.JobTargetCollegeId,

                JobId = target.JobId,

                CollegeId = target.CollegeId,

                CollegeName = college.CollegeName
            });
        }

        return response;
    }

    public async Task<bool> RemoveTargetAsync(
        int jobId,
        int collegeId,
        string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null ||
            job.Status != Models.Enums.JobStatus.Draft)
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

        var target =
            await _targetRepository.GetAsync(
                jobId,
                collegeId);

        if (target == null)
        {
            return false;
        }

        await _targetRepository.DeleteAsync(target);

        return true;
    }
}