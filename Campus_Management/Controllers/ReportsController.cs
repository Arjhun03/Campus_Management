using System.Security.Claims;
using Campus_Management.DTOs.Reports;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<AdminDashboardReport>> GetDashboardReport()
    {
        var report = await _reportService.GetAdminDashboardReportAsync();
        return Ok(report);
    }

    [HttpGet("colleges")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<List<CollegePlacementReport>>> GetAllCollegesReports()
    {
        var reports = await _reportService.GetAllCollegesPlacementReportsAsync();
        return Ok(reports);
    }

    [HttpGet("colleges/{collegeId:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<CollegePlacementReport>> GetCollegeReport(int collegeId)
    {
        var report = await _reportService.GetCollegePlacementReportAsync(collegeId);
        if (report == null)
        {
            return NotFound(new { message = "College report not found." });
        }

        return Ok(report);
    }

    [HttpGet("my-college")]
    [Authorize(Roles = "PlacementOfficer")]
    public async Task<ActionResult<CollegePlacementReport>> GetMyCollegeReport()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var report = await _reportService.GetPlacementOfficerCollegeReportAsync(userId);
        if (report == null)
        {
            return NotFound(new { message = "Placement officer profile or college not found." });
        }

        return Ok(report);
    }

    [HttpGet("companies")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<List<CompanyHiringReport>>> GetAllCompaniesReports()
    {
        var reports = await _reportService.GetAllCompaniesHiringReportsAsync();
        return Ok(reports);
    }

    [HttpGet("companies/{companyId:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<CompanyHiringReport>> GetCompanyReport(int companyId)
    {
        var report = await _reportService.GetCompanyHiringReportAsync(companyId);
        if (report == null)
        {
            return NotFound(new { message = "Company report not found." });
        }

        return Ok(report);
    }

    [HttpGet("my-company")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<ActionResult<CompanyHiringReport>> GetMyCompanyReport()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var report = await _reportService.GetCompanyHRReportAsync(userId);
        if (report == null)
        {
            return NotFound(new { message = "Company HR profile or company not found." });
        }

        return Ok(report);
    }

    [HttpGet("my-summary")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<StudentPlacementSummary>> GetMyPlacementSummary()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var report = await _reportService.GetStudentPlacementSummaryAsync(userId);
        if (report == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        return Ok(report);
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
