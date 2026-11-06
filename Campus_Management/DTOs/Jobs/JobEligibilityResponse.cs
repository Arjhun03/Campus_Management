namespace Campus_Management.DTOs.Jobs;

public class JobEligibilityResponse
{
    public int JobId { get; set; }

    public bool IsEligible { get; set; }

    public string Reason { get; set; } = string.Empty;
}