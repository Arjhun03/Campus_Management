using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.InterviewRounds;

public class CreateInterviewRoundRequest
{
    [Required]
    public int DriveId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Round number must be greater than 0.")]
    public int RoundNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string RoundName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RoundType { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}
