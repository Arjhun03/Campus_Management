using Campus_Management.DTOs.Reports;

namespace Campus_Management.Services.Interfaces;

public interface IReportService
{
    Task<AdminDashboardReport> GetAdminDashboardReportAsync();
    Task<List<CollegePlacementReport>> GetAllCollegesPlacementReportsAsync();
    Task<CollegePlacementReport?> GetCollegePlacementReportAsync(int collegeId);
    Task<CollegePlacementReport?> GetPlacementOfficerCollegeReportAsync(string userId);
    Task<List<CompanyHiringReport>> GetAllCompaniesHiringReportsAsync();
    Task<CompanyHiringReport?> GetCompanyHiringReportAsync(int companyId);
    Task<CompanyHiringReport?> GetCompanyHRReportAsync(string userId);
    Task<StudentPlacementSummary?> GetStudentPlacementSummaryAsync(string studentUserId);
}
