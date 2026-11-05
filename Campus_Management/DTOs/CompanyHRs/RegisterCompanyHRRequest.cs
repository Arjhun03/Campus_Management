using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.CompanyHRs;

public class RegisterCompanyHRRequest
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;
}