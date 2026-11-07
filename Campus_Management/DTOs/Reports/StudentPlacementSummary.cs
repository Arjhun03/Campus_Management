namespace Campus_Management.DTOs.Reports;

public class StudentPlacementSummary
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNumber { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public int TotalApplications { get; set; }
    public int UnderReviewCount { get; set; }
    public int ShortlistedCount { get; set; }
    public int InterviewCount { get; set; }
    public int SelectedCount { get; set; }
    public int RejectedCount { get; set; }
    public int WithdrawnCount { get; set; }
    public bool IsPlaced => SelectedCount > 0;
    public List<PlacedOfferDetail> Offers { get; set; } = new();
}

public class PlacedOfferDetail
{
    public int ApplicationId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal? SalaryPackage { get; set; }
    public DateTime SelectedAt { get; set; }
}
