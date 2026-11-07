using Campus_Management.DTOs.Reports;

namespace Campus_Management.Repositories.Interfaces;

public interface IReportRepository
{
    Task<AdminDashboardReport> GetAdminDashboardReportAsync();
    Task<List<CollegePlacementReport>> GetAllCollegesPlacementReportsAsync();
    Task<CollegePlacementReport?> GetCollegePlacementReportAsync(int collegeId);
    Task<List<CompanyHiringReport>> GetAllCompaniesHiringReportsAsync();
    Task<CompanyHiringReport?> GetCompanyHiringReportAsync(int companyId);
    Task<StudentPlacementSummary?> GetStudentPlacementSummaryAsync(int studentId);
}
