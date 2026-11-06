namespace Campus_Management.DTOs.Jobs;

public class JobTargetCollegeResponse
{
    public int JobTargetCollegeId { get; set; }

    public int JobId { get; set; }

    public int CollegeId { get; set; }

    public string CollegeName { get; set; } = string.Empty;

    public string CollegeCity { get; set; } = string.Empty;
}
