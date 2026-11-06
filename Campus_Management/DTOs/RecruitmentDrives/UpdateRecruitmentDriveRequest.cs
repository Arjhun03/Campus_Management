using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.RecruitmentDrives;

public class UpdateRecruitmentDriveRequest
{
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
    public DriveStatus Status { get; set; }

    [MaxLength(2000)]
    public string? Instructions { get; set; }
}
