using System.Security.Claims;
using Campus_Management.DTOs.Reports;
using Campus_Management.Repositories.Interfaces;
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
    private readonly IStudentRepository _studentRepository;

    public ReportsController(
        IReportService reportService,
        IStudentRepository studentRepository)
    {
        _reportService = reportService;
        _studentRepository = studentRepository;
    }

    [HttpGet("platform-overview")]
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<AdminDashboardReport>> GetDashboardReport()
    {
        var report = await _reportService.GetAdminDashboardReportAsync();
        return Ok(report);
    }

    [HttpGet("college-overview")]
    [Authorize(Roles = "PlacementOfficer,Admin,SuperAdmin")]
    public async Task<ActionResult<object>> GetCollegeOverview([FromQuery] int? collegeId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role == "PlacementOfficer")
        {
            var report = await _reportService.GetPlacementOfficerCollegeReportAsync(userId);
            if (report == null)
            {
                return NotFound(new { message = "Placement officer profile or college not found." });
            }

            return Ok(report);
        }

        if (collegeId.HasValue && collegeId.Value > 0)
        {
            var report = await _reportService.GetCollegePlacementReportAsync(collegeId.Value);
            if (report == null)
            {
                return NotFound(new { message = "College report not found." });
            }

            return Ok(report);
        }

        var reports = await _reportService.GetAllCollegesPlacementReportsAsync();
        return Ok(reports);
    }

    [HttpGet("company-recruitment")]
    [Authorize(Roles = "CompanyHR,Admin,SuperAdmin")]
    public async Task<ActionResult<object>> GetCompanyRecruitment([FromQuery] int? companyId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role == "CompanyHR")
        {
            var report = await _reportService.GetCompanyHRReportAsync(userId);
            if (report == null)
            {
                return NotFound(new { message = "Company HR profile or company not found." });
            }

            return Ok(report);
        }

        if (companyId.HasValue && companyId.Value > 0)
        {
            var report = await _reportService.GetCompanyHiringReportAsync(companyId.Value);
            if (report == null)
            {
                return NotFound(new { message = "Company report not found." });
            }

            return Ok(report);
        }

        var reports = await _reportService.GetAllCompaniesHiringReportsAsync();
        return Ok(reports);
    }

    [HttpGet("student-placement")]
    [Authorize(Roles = "Student,Admin,SuperAdmin")]
    public async Task<ActionResult<StudentPlacementSummary>> GetStudentPlacement([FromQuery] int? studentId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role == "Student")
        {
            var report = await _reportService.GetStudentPlacementSummaryAsync(userId);
            if (report == null)
            {
                return NotFound(new { message = "Student profile not found." });
            }

            return Ok(report);
        }

        if (studentId.HasValue && studentId.Value > 0)
        {
            var student = await _studentRepository.GetByIdAsync(studentId.Value);
            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            var report = await _reportService.GetStudentPlacementSummaryAsync(student.UserId);
            if (report == null)
            {
                return NotFound(new { message = "Student placement summary not found." });
            }

            return Ok(report);
        }

        return BadRequest(new { message = "studentId query parameter is required for Admin access." });
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
