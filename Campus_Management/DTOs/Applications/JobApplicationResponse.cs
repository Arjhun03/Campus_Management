using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.Applications;

public class JobApplicationResponse
{
    public int ApplicationId { get; set; }

    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;

    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string RegisterNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public decimal CGPA { get; set; }

    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;

    public string? ResumeUrl { get; set; }

    public DateTime AppliedAt { get; set; }
    public ApplicationStatus Status { get; set; }
    public string? Remarks { get; set; }
}
