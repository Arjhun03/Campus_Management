using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.InterviewSchedules;

public class InterviewFeedbackRequest
{
    [Required]
    public InterviewStatus Status { get; set; } = InterviewStatus.Completed;

    [MaxLength(2000)]
    public string? Feedback { get; set; }

    [Range(0, 100)]
    public decimal? Score { get; set; }
}
