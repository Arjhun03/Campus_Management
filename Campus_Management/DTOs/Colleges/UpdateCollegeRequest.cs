using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.Colleges;

public class UpdateCollegeRequest
{
    [Required]
    [MaxLength(150)]
    public string CollegeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public int Status { get; set; }
}