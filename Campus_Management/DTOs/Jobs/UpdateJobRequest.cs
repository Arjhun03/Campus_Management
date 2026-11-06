using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.Jobs;

public class UpdateJobRequest
{
    [Required]
    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public string JobDescription { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string JobType { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0, 9999999999.99)]
    public decimal? SalaryPackage { get; set; }

    [Range(0, 10)]
    public decimal? MinimumCGPA { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaximumBacklogs { get; set; }

    [Required]
    public DateTime ApplicationDeadline { get; set; }

    [Required]
    public bool IsOpenToAllColleges { get; set; }
}