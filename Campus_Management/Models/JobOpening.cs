using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.Models;

public class JobOpening
{
    [Key]
    public int JobId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    public int CreatedByHRId { get; set; }

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

    public decimal? SalaryPackage { get; set; }

    [Range(0, 10)]
    public decimal? MinimumCGPA { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaximumBacklogs { get; set; }

    [Required]
    public DateTime ApplicationDeadline { get; set; }

    [Required]
    public bool IsOpenToAllColleges { get; set; }

    [Required]
    public JobStatus Status { get; set; } = JobStatus.Draft;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}