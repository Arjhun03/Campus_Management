namespace Campus_Management.DTOs.Reports;

public class CollegePlacementReport
{
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int PlacedStudents { get; set; }
    public decimal PlacementRatePercentage { get; set; }
    public decimal AveragePackage { get; set; }
    public decimal HighestPackage { get; set; }
    public List<DepartmentPlacementStat> DepartmentStats { get; set; } = new();
}
