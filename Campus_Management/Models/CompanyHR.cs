using System.ComponentModel.DataAnnotations;

namespace Campus_Management.Models;

public class CompanyHR
{
    [Key]
    public int CompanyHRId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
}