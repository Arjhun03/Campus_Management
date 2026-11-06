using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.Models;

public class Student
{
    [Key]
    public int StudentId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int CollegeId { get; set; }

    [Required]
    public int PlacementOfficerId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RegisterNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Degree { get; set; } = string.Empty;

    [Required]
    public int GraduationYear { get; set; }

    [Required]
    public decimal CGPA { get; set; }

    [Required]
    public int ActiveBacklogs { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }

    [Required]
    public StudentStatus Status { get; set; } = StudentStatus.Pending;

    public DateTime? ApprovedAt { get; set; }
}