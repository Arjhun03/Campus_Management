namespace Campus_Management.DTOs.InterviewRounds;

public class InterviewRoundResponse
{
    public int RoundId { get; set; }

    public int DriveId { get; set; }

    public string DriveName { get; set; } = string.Empty;

    public int JobId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public int RoundNumber { get; set; }

    public string RoundName { get; set; } = string.Empty;

    public string RoundType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
