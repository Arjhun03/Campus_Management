using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.InterviewSchedules;

public class InterviewScheduleResponse
{
    public int ScheduleId { get; set; }

    public int RoundId { get; set; }
    public int RoundNumber { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public string RoundType { get; set; } = string.Empty;

    public int DriveId { get; set; }
    public string DriveName { get; set; } = string.Empty;

    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;

    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    public int ApplicationId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentRegisterNumber { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;

    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
    public string? InterviewerEmail { get; set; }
    public string MeetingLinkOrLocation { get; set; } = string.Empty;
    public InterviewStatus Status { get; set; }
    public string? Feedback { get; set; }
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
