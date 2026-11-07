namespace Campus_Management.DTOs.Reports;

public class DepartmentPlacementStat
{
    public string Department { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int PlacedStudents { get; set; }
    public decimal PlacementRatePercentage { get; set; }
}
