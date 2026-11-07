namespace Campus_Management.DTOs.Reports;

public class CompanyHiringReport
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public int TotalJobsPosted { get; set; }
    public int TotalApplicationsReceived { get; set; }
    public int TotalShortlisted { get; set; }
    public int TotalInterviewed { get; set; }
    public int TotalHired { get; set; }
    public int TotalDrivesConducted { get; set; }
}
