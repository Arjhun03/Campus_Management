using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.Students;

public class UpdateStudentRequest
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Degree { get; set; } = string.Empty;

    [Required]
    public int GraduationYear { get; set; }

    [Required]
    [Range(0, 10)]
    public decimal CGPA { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int ActiveBacklogs { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }
}