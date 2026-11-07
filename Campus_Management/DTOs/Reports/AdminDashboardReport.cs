namespace Campus_Management.DTOs.Reports;

public class AdminDashboardReport
{
    public int TotalColleges { get; set; }
    public int ActiveColleges { get; set; }
    public int TotalCompanies { get; set; }
    public int ActiveCompanies { get; set; }
    public int TotalStudents { get; set; }
    public int ApprovedStudents { get; set; }
    public int TotalJobs { get; set; }
    public int PublishedJobs { get; set; }
    public int TotalApplications { get; set; }
    public int TotalSelectedStudents { get; set; }
    public decimal OverallPlacementRatePercentage { get; set; }
    public decimal AveragePackageOffered { get; set; }
    public decimal HighestPackageOffered { get; set; }
}
