using Campus_Management.DTOs.Reports;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportService(
        IReportRepository reportRepository,
        IStudentRepository studentRepository,
        ICompanyHRRepository companyHRRepository,
        IPlacementOfficerRepository placementOfficerRepository,
        UserManager<ApplicationUser> userManager)
    {
        _reportRepository = reportRepository;
        _studentRepository = studentRepository;
        _companyHRRepository = companyHRRepository;
        _placementOfficerRepository = placementOfficerRepository;
        _userManager = userManager;
    }

    public async Task<AdminDashboardReport> GetAdminDashboardReportAsync()
    {
        return await _reportRepository.GetAdminDashboardReportAsync();
    }

    public async Task<List<CollegePlacementReport>> GetAllCollegesPlacementReportsAsync()
    {
        return await _reportRepository.GetAllCollegesPlacementReportsAsync();
    }

    public async Task<CollegePlacementReport?> GetCollegePlacementReportAsync(int collegeId)
    {
        return await _reportRepository.GetCollegePlacementReportAsync(collegeId);
    }

    public async Task<CollegePlacementReport?> GetPlacementOfficerCollegeReportAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var officer = await _placementOfficerRepository.GetByEmailAsync(user.Email ?? string.Empty);
        if (officer == null)
        {
            return null;
        }

        return await _reportRepository.GetCollegePlacementReportAsync(officer.CollegeId);
    }

    public async Task<List<CompanyHiringReport>> GetAllCompaniesHiringReportsAsync()
    {
        return await _reportRepository.GetAllCompaniesHiringReportsAsync();
    }

    public async Task<CompanyHiringReport?> GetCompanyHiringReportAsync(int companyId)
    {
        return await _reportRepository.GetCompanyHiringReportAsync(companyId);
    }

    public async Task<CompanyHiringReport?> GetCompanyHRReportAsync(string userId)
    {
        var hr = await _companyHRRepository.GetByUserIdAsync(userId);
        if (hr == null)
        {
            return null;
        }

        return await _reportRepository.GetCompanyHiringReportAsync(hr.CompanyId);
    }

    public async Task<StudentPlacementSummary?> GetStudentPlacementSummaryAsync(string studentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId);
        if (student == null)
        {
            return null;
        }

        return await _reportRepository.GetStudentPlacementSummaryAsync(student.StudentId);
    }
}
