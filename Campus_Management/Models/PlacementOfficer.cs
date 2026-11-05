namespace Campus_Management.Models;

public class PlacementOfficer
{
    public int PlacementOfficerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public int CollegeId { get; set; }

    public College College { get; set; } = null!;
}