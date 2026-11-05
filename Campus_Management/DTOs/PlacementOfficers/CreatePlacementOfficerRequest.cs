using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.PlacementOfficers;

public class CreatePlacementOfficerRequest
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public int CollegeId { get; set; }
}