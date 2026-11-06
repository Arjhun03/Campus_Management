using System.ComponentModel.DataAnnotations;

namespace Campus_Management.Models;

public class InterviewRound
{
    [Key]
    public int RoundId { get; set; }

    [Required]
    public int DriveId { get; set; }

    [Required]
    public int RoundNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string RoundName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RoundType { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
