using Campus_Management.DTOs.InterviewSchedules;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class InterviewScheduleService : IInterviewScheduleService
{
    private readonly IInterviewScheduleRepository _scheduleRepository;
    private readonly IInterviewRoundRepository _roundRepository;
    private readonly IRecruitmentDriveRepository _driveRepository;
    private readonly IJobApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly IJobTargetCollegeRepository _targetRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;
    private readonly IApplicationStatusHistoryRepository _historyRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public InterviewScheduleService(
        IInterviewScheduleRepository scheduleRepository,
        IInterviewRoundRepository roundRepository,
        IRecruitmentDriveRepository driveRepository,
        IJobApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IStudentRepository studentRepository,
        ICollegeRepository collegeRepository,
        ICompanyRepository companyRepository,
        ICompanyHRRepository companyHRRepository,
        IJobTargetCollegeRepository targetRepository,
        IPlacementOfficerRepository placementOfficerRepository,
        IApplicationStatusHistoryRepository historyRepository,
        UserManager<ApplicationUser> userManager)
    {
        _scheduleRepository = scheduleRepository;
        _roundRepository = roundRepository;
        _driveRepository = driveRepository;
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _studentRepository = studentRepository;
        _collegeRepository = collegeRepository;
        _companyRepository = companyRepository;
        _companyHRRepository = companyHRRepository;
        _targetRepository = targetRepository;
        _placementOfficerRepository = placementOfficerRepository;
        _historyRepository = historyRepository;
        _userManager = userManager;
    }

    public async Task<InterviewScheduleResponse?> CreateAsync(
        CreateInterviewScheduleRequest request,
        string userId)
    {
        if (request.ScheduledEndTime <= request.ScheduledStartTime)
        {
            return null;
        }

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

        var round = await _roundRepository.GetByIdAsync(request.RoundId);
        if (round == null)
        {
            return null;
        }

        var drive = await _driveRepository.GetByIdAsync(round.DriveId);
        if (drive == null || drive.CompanyId != companyHR.CompanyId)
        {
            return null;
        }

        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId);
        if (application == null || application.JobId != drive.JobId)
        {
            return null;
        }

        if (application.Status == ApplicationStatus.Rejected || application.Status == ApplicationStatus.Withdrawn)
        {
            return null;
        }

        var existingSchedule = await _scheduleRepository.GetByRoundAndApplicationAsync(request.RoundId, request.ApplicationId);
        if (existingSchedule != null)
        {
            return null;
        }

        var schedule = new InterviewSchedule
        {
            RoundId = request.RoundId,
            ApplicationId = request.ApplicationId,
            ScheduledStartTime = request.ScheduledStartTime,
            ScheduledEndTime = request.ScheduledEndTime,
            InterviewerName = request.InterviewerName,
            InterviewerEmail = request.InterviewerEmail,
            MeetingLinkOrLocation = request.MeetingLinkOrLocation,
            Status = InterviewStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        await _scheduleRepository.AddAsync(schedule);

        if (application.Status != ApplicationStatus.Interview)
        {
            var oldStatus = application.Status;
            application.Status = ApplicationStatus.Interview;
            await _applicationRepository.UpdateAsync(application);

            await _historyRepository.AddAsync(new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                FromStatus = oldStatus,
                ToStatus = ApplicationStatus.Interview,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Remarks = $"Scheduled for Interview Round {round.RoundNumber}: {round.RoundName}"
            });
        }

        return await BuildResponseAsync(schedule, round, drive, application);
    }

    public async Task<InterviewScheduleResponse?> GetByIdAsync(
        int id,
        string currentUserId,
        string role)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return null;
        }

        var round = await _roundRepository.GetByIdAsync(schedule.RoundId);
        if (round == null)
        {
            return null;
        }

        var drive = await _driveRepository.GetByIdAsync(round.DriveId);
        if (drive == null)
        {
            return null;
        }

        var application = await _applicationRepository.GetByIdAsync(schedule.ApplicationId);
        if (application == null)
        {
            return null;
        }

        var hasAccess = await HasAccessToScheduleAsync(drive, application, currentUserId, role);
        if (!hasAccess)
        {
            return null;
        }

        return await BuildResponseAsync(schedule, round, drive, application);
    }

    public async Task<List<InterviewScheduleResponse>> GetByRoundIdAsync(
        int roundId,
        string currentUserId,
        string role)
    {
        var round = await _roundRepository.GetByIdAsync(roundId);
        if (round == null)
        {
            return new List<InterviewScheduleResponse>();
        }

        var drive = await _driveRepository.GetByIdAsync(round.DriveId);
        if (drive == null)
        {
            return new List<InterviewScheduleResponse>();
        }

        if (role == "CompanyHR")
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
            if (hr == null || hr.CompanyId != drive.CompanyId)
            {
                return new List<InterviewScheduleResponse>();
            }
        }
        else if (role == "PlacementOfficer")
        {
            var user = await _userManager.FindByIdAsync(currentUserId);
            var officer = await _placementOfficerRepository.GetByEmailAsync(user?.Email ?? string.Empty);
            if (officer == null)
            {
                return new List<InterviewScheduleResponse>();
            }

            var targets = await _targetRepository.GetByJobIdAsync(drive.JobId);
            if (!targets.Any(t => t.CollegeId == officer.CollegeId))
            {
                return new List<InterviewScheduleResponse>();
            }
        }
        else if (role != "Admin" && role != "SuperAdmin")
        {
            return new List<InterviewScheduleResponse>();
        }

        var schedules = await _scheduleRepository.GetByRoundIdAsync(roundId);
        var responses = new List<InterviewScheduleResponse>();

        foreach (var schedule in schedules)
        {
            var application = await _applicationRepository.GetByIdAsync(schedule.ApplicationId);
            if (application != null)
            {
                var res = await BuildResponseAsync(schedule, round, drive, application);
                if (res != null)
                {
                    responses.Add(res);
                }
            }
        }

        return responses;
    }

    public async Task<List<InterviewScheduleResponse>> GetByApplicationIdAsync(
        int applicationId,
        string currentUserId,
        string role)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
        {
            return new List<InterviewScheduleResponse>();
        }

        var job = await _jobRepository.GetByIdAsync(application.JobId);
        if (job == null)
        {
            return new List<InterviewScheduleResponse>();
        }

        if (role == "Student")
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null || student.StudentId != application.StudentId)
            {
                return new List<InterviewScheduleResponse>();
            }
        }
        else if (role == "CompanyHR")
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return new List<InterviewScheduleResponse>();
            }
        }
        else if (role == "PlacementOfficer")
        {
            var user = await _userManager.FindByIdAsync(currentUserId);
            var officer = await _placementOfficerRepository.GetByEmailAsync(user?.Email ?? string.Empty);
            var student = await _studentRepository.GetByIdAsync(application.StudentId);
            if (officer == null || student == null || student.CollegeId != officer.CollegeId)
            {
                return new List<InterviewScheduleResponse>();
            }
        }

        var schedules = await _scheduleRepository.GetByApplicationIdAsync(applicationId);
        var responses = new List<InterviewScheduleResponse>();

        foreach (var schedule in schedules)
        {
            var round = await _roundRepository.GetByIdAsync(schedule.RoundId);
            if (round != null)
            {
                var drive = await _driveRepository.GetByIdAsync(round.DriveId);
                if (drive != null)
                {
                    var res = await BuildResponseAsync(schedule, round, drive, application);
                    if (res != null)
                    {
                        responses.Add(res);
                    }
                }
            }
        }

        return responses;
    }

    public async Task<List<InterviewScheduleResponse>> GetMySchedulesAsync(string currentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(currentUserId);
        if (student == null)
        {
            return new List<InterviewScheduleResponse>();
        }

        var schedules = await _scheduleRepository.GetByStudentIdAsync(student.StudentId);
        var responses = new List<InterviewScheduleResponse>();

        foreach (var schedule in schedules)
        {
            var application = await _applicationRepository.GetByIdAsync(schedule.ApplicationId);
            var round = await _roundRepository.GetByIdAsync(schedule.RoundId);
            if (application != null && round != null)
            {
                var drive = await _driveRepository.GetByIdAsync(round.DriveId);
                if (drive != null)
                {
                    var res = await BuildResponseAsync(schedule, round, drive, application);
                    if (res != null)
                    {
                        responses.Add(res);
                    }
                }
            }
        }

        return responses;
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateInterviewScheduleRequest request,
        string userId)
    {
        if (request.ScheduledEndTime <= request.ScheduledStartTime)
        {
            return false;
        }

        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return false;
        }

        var round = await _roundRepository.GetByIdAsync(schedule.RoundId);
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

        schedule.ScheduledStartTime = request.ScheduledStartTime;
        schedule.ScheduledEndTime = request.ScheduledEndTime;
        schedule.InterviewerName = request.InterviewerName;
        schedule.InterviewerEmail = request.InterviewerEmail;
        schedule.MeetingLinkOrLocation = request.MeetingLinkOrLocation;
        schedule.Status = request.Status;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule);

        return true;
    }

    public async Task<bool> SubmitFeedbackAsync(
        int id,
        InterviewFeedbackRequest request,
        string userId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return false;
        }

        var round = await _roundRepository.GetByIdAsync(schedule.RoundId);
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

        schedule.Status = request.Status;
        schedule.Feedback = request.Feedback;
        schedule.Score = request.Score;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return false;
        }

        var round = await _roundRepository.GetByIdAsync(schedule.RoundId);
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

        await _scheduleRepository.DeleteAsync(schedule);

        return true;
    }

    private async Task<bool> HasAccessToScheduleAsync(
        RecruitmentDrive drive,
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
            var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
            return hr != null && hr.CompanyId == drive.CompanyId;
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

    private async Task<InterviewScheduleResponse?> BuildResponseAsync(
        InterviewSchedule schedule,
        InterviewRound round,
        RecruitmentDrive drive,
        JobApplication application)
    {
        var job = await _jobRepository.GetByIdAsync(drive.JobId);
        var company = await _companyRepository.GetByIdAsync(drive.CompanyId);
        var student = await _studentRepository.GetByIdAsync(application.StudentId);
        var college = student != null ? await _collegeRepository.GetByIdAsync(student.CollegeId) : null;
        var studentUser = student != null ? await _userManager.FindByIdAsync(student.UserId) : null;

        return new InterviewScheduleResponse
        {
            ScheduleId = schedule.ScheduleId,
            RoundId = round.RoundId,
            RoundNumber = round.RoundNumber,
            RoundName = round.RoundName,
            RoundType = round.RoundType,
            DriveId = drive.DriveId,
            DriveName = drive.DriveName,
            JobId = drive.JobId,
            JobTitle = job?.JobTitle ?? string.Empty,
            CompanyId = drive.CompanyId,
            CompanyName = company?.CompanyName ?? string.Empty,
            ApplicationId = application.ApplicationId,
            StudentId = application.StudentId,
            StudentName = studentUser?.FullName ?? string.Empty,
            StudentRegisterNumber = student?.RegisterNumber ?? string.Empty,
            StudentEmail = studentUser?.Email ?? string.Empty,
            CollegeId = student?.CollegeId ?? 0,
            CollegeName = college?.CollegeName ?? string.Empty,
            ScheduledStartTime = schedule.ScheduledStartTime,
            ScheduledEndTime = schedule.ScheduledEndTime,
            InterviewerName = schedule.InterviewerName,
            InterviewerEmail = schedule.InterviewerEmail,
            MeetingLinkOrLocation = schedule.MeetingLinkOrLocation,
            Status = schedule.Status,
            Feedback = schedule.Feedback,
            Score = schedule.Score,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }
}
