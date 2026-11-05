using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.Models;

public class College
{
    public int CollegeId { get; set; }

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
    public string Email { get; set; } = string.Empty;

    public CollegeStatus Status { get; set; } = CollegeStatus.Active;
}