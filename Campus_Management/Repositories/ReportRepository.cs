using Campus_Management.Data;
using Campus_Management.DTOs.Reports;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportRepository(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminDashboardReport> GetAdminDashboardReportAsync()
    {
        var totalColleges = await _context.Colleges.CountAsync();
        var activeColleges = await _context.Colleges.CountAsync(c => c.Status == CollegeStatus.Active);

        var totalCompanies = await _context.Companies.CountAsync();
        var activeCompanies = await _context.Companies.CountAsync(c => c.Status == CompanyStatus.Active);

        var totalStudents = await _context.Students.CountAsync();
        var approvedStudents = await _context.Students.CountAsync(s => s.Status == StudentStatus.Approved);

        var totalJobs = await _context.JobOpenings.CountAsync();
        var publishedJobs = await _context.JobOpenings.CountAsync(j => j.Status == JobStatus.Published);

        var totalApplications = await _context.JobApplications.CountAsync();

        var selectedStudentsCount = await _context.JobApplications
            .Where(a => a.Status == ApplicationStatus.Selected)
            .Select(a => a.StudentId)
            .Distinct()
            .CountAsync();

        var placementRate = approvedStudents > 0
            ? Math.Round((decimal)selectedStudentsCount / approvedStudents * 100m, 2)
            : 0m;

        var selectedPackages = await (from a in _context.JobApplications
                                      join j in _context.JobOpenings on a.JobId equals j.JobId
                                      where a.Status == ApplicationStatus.Selected && j.SalaryPackage.HasValue
                                      select j.SalaryPackage.Value).ToListAsync();

        var avgPackage = selectedPackages.Count > 0
            ? Math.Round(selectedPackages.Average(), 2)
            : 0m;

        var highestPackage = selectedPackages.Count > 0
            ? selectedPackages.Max()
            : 0m;

        return new AdminDashboardReport
        {
            TotalColleges = totalColleges,
            ActiveColleges = activeColleges,
            TotalCompanies = totalCompanies,
            ActiveCompanies = activeCompanies,
            TotalStudents = totalStudents,
            ApprovedStudents = approvedStudents,
            TotalJobs = totalJobs,
            PublishedJobs = publishedJobs,
            TotalApplications = totalApplications,
            TotalSelectedStudents = selectedStudentsCount,
            OverallPlacementRatePercentage = placementRate,
            AveragePackageOffered = avgPackage,
            HighestPackageOffered = highestPackage
        };
    }

    public async Task<List<CollegePlacementReport>> GetAllCollegesPlacementReportsAsync()
    {
        var colleges = await _context.Colleges.OrderBy(c => c.CollegeName).ToListAsync();
        var reports = new List<CollegePlacementReport>();

        foreach (var college in colleges)
        {
            var rep = await BuildCollegeReportAsync(college);
            reports.Add(rep);
        }

        return reports;
    }

    public async Task<CollegePlacementReport?> GetCollegePlacementReportAsync(int collegeId)
    {
        var college = await _context.Colleges.FirstOrDefaultAsync(c => c.CollegeId == collegeId);
        if (college == null)
        {
            return null;
        }

        return await BuildCollegeReportAsync(college);
    }

    public async Task<List<CompanyHiringReport>> GetAllCompaniesHiringReportsAsync()
    {
        var companies = await _context.Companies.OrderBy(c => c.CompanyName).ToListAsync();
        var reports = new List<CompanyHiringReport>();

        foreach (var company in companies)
        {
            var rep = await BuildCompanyReportAsync(company);
            reports.Add(rep);
        }

        return reports;
    }

    public async Task<CompanyHiringReport?> GetCompanyHiringReportAsync(int companyId)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        if (company == null)
        {
            return null;
        }

        return await BuildCompanyReportAsync(company);
    }

    public async Task<StudentPlacementSummary?> GetStudentPlacementSummaryAsync(int studentId)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
        if (student == null)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(student.UserId);
        var college = await _context.Colleges.FirstOrDefaultAsync(c => c.CollegeId == student.CollegeId);

        var applications = await _context.JobApplications
            .Where(a => a.StudentId == studentId)
            .ToListAsync();

        var offersQuery = from a in _context.JobApplications
                          join j in _context.JobOpenings on a.JobId equals j.JobId
                          join c in _context.Companies on j.CompanyId equals c.CompanyId
                          where a.StudentId == studentId && a.Status == ApplicationStatus.Selected
                          select new PlacedOfferDetail
                          {
                              ApplicationId = a.ApplicationId,
                              CompanyName = c.CompanyName,
                              JobTitle = j.JobTitle,
                              SalaryPackage = j.SalaryPackage,
                              SelectedAt = a.AppliedAt
                          };

        var offers = await offersQuery.ToListAsync();

        return new StudentPlacementSummary
        {
            StudentId = student.StudentId,
            StudentName = user?.FullName ?? string.Empty,
            RegisterNumber = student.RegisterNumber,
            CollegeName = college?.CollegeName ?? string.Empty,
            TotalApplications = applications.Count,
            UnderReviewCount = applications.Count(a => a.Status == ApplicationStatus.UnderReview),
            ShortlistedCount = applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
            InterviewCount = applications.Count(a => a.Status == ApplicationStatus.Interview),
            SelectedCount = applications.Count(a => a.Status == ApplicationStatus.Selected),
            RejectedCount = applications.Count(a => a.Status == ApplicationStatus.Rejected),
            WithdrawnCount = applications.Count(a => a.Status == ApplicationStatus.Withdrawn),
            Offers = offers
        };
    }

    private async Task<CollegePlacementReport> BuildCollegeReportAsync(College college)
    {
        var students = await _context.Students
            .Where(s => s.CollegeId == college.CollegeId && s.Status == StudentStatus.Approved)
            .ToListAsync();

        var placedStudentIds = await (from a in _context.JobApplications
                                      join s in _context.Students on a.StudentId equals s.StudentId
                                      where s.CollegeId == college.CollegeId && a.Status == ApplicationStatus.Selected
                                      select s.StudentId).Distinct().ToListAsync();

        var placementRate = students.Count > 0
            ? Math.Round((decimal)placedStudentIds.Count / students.Count * 100m, 2)
            : 0m;

        var placedPackages = await (from a in _context.JobApplications
                                    join s in _context.Students on a.StudentId equals s.StudentId
                                    join j in _context.JobOpenings on a.JobId equals j.JobId
                                    where s.CollegeId == college.CollegeId && a.Status == ApplicationStatus.Selected && j.SalaryPackage.HasValue
                                    select j.SalaryPackage.Value).ToListAsync();

        var avgPkg = placedPackages.Count > 0 ? Math.Round(placedPackages.Average(), 2) : 0m;
        var maxPkg = placedPackages.Count > 0 ? placedPackages.Max() : 0m;

        var deptStats = new List<DepartmentPlacementStat>();
        var groupedByDept = students.GroupBy(s => s.Department);

        foreach (var group in groupedByDept)
        {
            var deptTotal = group.Count();
            var deptPlaced = group.Count(s => placedStudentIds.Contains(s.StudentId));
            var deptRate = deptTotal > 0 ? Math.Round((decimal)deptPlaced / deptTotal * 100m, 2) : 0m;

            deptStats.Add(new DepartmentPlacementStat
            {
                Department = group.Key,
                TotalStudents = deptTotal,
                PlacedStudents = deptPlaced,
                PlacementRatePercentage = deptRate
            });
        }

        return new CollegePlacementReport
        {
            CollegeId = college.CollegeId,
            CollegeName = college.CollegeName,
            City = college.City,
            TotalStudents = students.Count,
            PlacedStudents = placedStudentIds.Count,
            PlacementRatePercentage = placementRate,
            AveragePackage = avgPkg,
            HighestPackage = maxPkg,
            DepartmentStats = deptStats
        };
    }

    private async Task<CompanyHiringReport> BuildCompanyReportAsync(Company company)
    {
        var jobIds = await _context.JobOpenings
            .Where(j => j.CompanyId == company.CompanyId)
            .Select(j => j.JobId)
            .ToListAsync();

        var applications = await _context.JobApplications
            .Where(a => jobIds.Contains(a.JobId))
            .ToListAsync();

        var drivesCount = await _context.RecruitmentDrives
            .CountAsync(d => d.CompanyId == company.CompanyId);

        return new CompanyHiringReport
        {
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName,
            Industry = company.Industry,
            TotalJobsPosted = jobIds.Count,
            TotalApplicationsReceived = applications.Count,
            TotalShortlisted = applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
            TotalInterviewed = applications.Count(a => a.Status == ApplicationStatus.Interview),
            TotalHired = applications.Count(a => a.Status == ApplicationStatus.Selected),
            TotalDrivesConducted = drivesCount
        };
    }
}
