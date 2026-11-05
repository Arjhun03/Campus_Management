using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.Colleges;

public class CollegeResponse
{
    public int CollegeId { get; set; }

    public string CollegeName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public CollegeStatus Status { get; set; }
}