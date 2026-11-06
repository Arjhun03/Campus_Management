using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Campus_Management.Models.Enums;

namespace Campus_Management.Models;

public class InterviewSchedule
{
    [Key]
    public int ScheduleId { get; set; }

    [Required]
    public int RoundId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public DateTime ScheduledStartTime { get; set; }

    [Required]
    public DateTime ScheduledEndTime { get; set; }

    [Required]
    [MaxLength(150)]
    public string InterviewerName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? InterviewerEmail { get; set; }

    [Required]
    [MaxLength(500)]
    public string MeetingLinkOrLocation { get; set; } = string.Empty;

    [Required]
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    [MaxLength(2000)]
    public string? Feedback { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Score { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
