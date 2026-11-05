namespace Campus_Management.DTOs.PlacementOfficers;

public class PlacementOfficerResponse
{
    public int PlacementOfficerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public int CollegeId { get; set; }

    public string CollegeName { get; set; } = string.Empty;
}