using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.Students;

public class StudentResponse
{
    public int StudentId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public int CollegeId { get; set; }

    public string CollegeName { get; set; } = string.Empty;

    public int PlacementOfficerId { get; set; }

    public string PlacementOfficerName { get; set; } = string.Empty;

    public string RegisterNumber { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Degree { get; set; } = string.Empty;

    public int GraduationYear { get; set; }

    public decimal CGPA { get; set; }

    public int ActiveBacklogs { get; set; }

    public string? ResumeUrl { get; set; }

    public StudentStatus Status { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsActive { get; set; }
}