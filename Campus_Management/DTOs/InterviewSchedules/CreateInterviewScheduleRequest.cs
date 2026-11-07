using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.InterviewSchedules;

public class CreateInterviewScheduleRequest
{
    [Required]
    public int RoundId { get; set; }

    public int ApplicationId { get; set; }

    [Required]
    public DateTime ScheduledStartTime { get; set; }

    [Required]
    public DateTime ScheduledEndTime { get; set; }

    [Required]
    [MaxLength(150)]
    public string InterviewerName { get; set; } = string.Empty;

    [MaxLength(150)]
    [EmailAddress]
    public string? InterviewerEmail { get; set; }

    [Required]
    [MaxLength(500)]
    public string MeetingLinkOrLocation { get; set; } = string.Empty;
}
