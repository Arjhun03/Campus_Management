using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.InterviewSchedules;

public class UpdateInterviewScheduleRequest
{
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

    [Required]
    public InterviewStatus Status { get; set; }
}
