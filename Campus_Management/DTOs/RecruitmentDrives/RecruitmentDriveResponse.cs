using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.RecruitmentDrives;

public class RecruitmentDriveResponse
{
    public int DriveId { get; set; }

    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;

    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    public string DriveName { get; set; } = string.Empty;
    public DateTime DriveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string DriveMode { get; set; } = string.Empty;
    public DriveStatus Status { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }
}
