using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.Models;

public class JobApplication
{
    [Key]
    public int ApplicationId { get; set; }

    [Required]
    public int JobId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    [MaxLength(1000)]
    public string? Remarks { get; set; }
}
