using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.Models;

public class RecruitmentDrive
{
    [Key]
    public int DriveId { get; set; }

    [Required]
    public int JobId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string DriveName { get; set; } = string.Empty;

    [Required]
    public DateTime DriveDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(250)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string DriveMode { get; set; } = string.Empty;

    [Required]
    public DriveStatus Status { get; set; } = DriveStatus.Scheduled;

    [MaxLength(2000)]
    public string? Instructions { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
