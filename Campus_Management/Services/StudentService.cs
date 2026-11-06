using Campus_Management.DTOs.Students;
using Campus_Management.Models;
using Campus_Management.Models.Enums;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICollegeRepository _collegeRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;

    public StudentService(
        IStudentRepository studentRepository,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ICollegeRepository collegeRepository,
        IPlacementOfficerRepository placementOfficerRepository)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _collegeRepository = collegeRepository;
        _placementOfficerRepository = placementOfficerRepository;
    }

    public async Task<StudentResponse?> EnrollAsync(
        EnrollStudentRequest request,
        string placementOfficerUserId)
    {
        var officerUser =
            await _userManager.FindByIdAsync(placementOfficerUserId);

        if (officerUser == null ||
            !officerUser.IsActive)
        {
            return null;
        }

        var isPlacementOfficer =
            await _userManager.IsInRoleAsync(officerUser, "PlacementOfficer");

        if (isPlacementOfficer)
        {
            var officer =
                await _placementOfficerRepository.GetByEmailAsync(
                    officerUser.Email ?? string.Empty);

            if (officer == null ||
                officer.CollegeId != request.CollegeId)
            {
                return null;
            }

            request.PlacementOfficerId = officer.PlacementOfficerId;
        }
        else
        {
            var college =
                await _collegeRepository.GetByIdAsync(request.CollegeId);

            if (college == null)
            {
                return null;
            }

            var officer =
                await _placementOfficerRepository.GetByIdAsync(
                    request.PlacementOfficerId);

            if (officer == null ||
                officer.CollegeId != request.CollegeId)
            {
                return null;
            }
        }

        if (await _studentRepository.RegisterNumberExistsAsync(
                request.CollegeId,
                request.RegisterNumber))
        {
            return null;
        }

        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true,
            IsActive = false
        };

        var userResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!userResult.Succeeded)
        {
            return null;
        }

        const string studentRole = "Student";

        if (!await _roleManager.RoleExistsAsync(studentRole))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(studentRole));
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                studentRole);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return null;
        }

        var student = new Student
        {
            UserId = user.Id,
            CollegeId = request.CollegeId,
            PlacementOfficerId = request.PlacementOfficerId,
            RegisterNumber = request.RegisterNumber,
            Department = request.Department,
            Degree = request.Degree,
            GraduationYear = request.GraduationYear,
            CGPA = request.CGPA,
            ActiveBacklogs = request.ActiveBacklogs,
            ResumeUrl = request.ResumeUrl,
            Status = StudentStatus.Pending
        };

        try
        {
            await _studentRepository.AddAsync(student);
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        return await GetByIdAsync(student.StudentId);
    }

    public async Task<List<StudentResponse>> BulkEnrollAsync(
        List<EnrollStudentRequest> requests,
        string placementOfficerUserId)
    {
        var results = new List<StudentResponse>();

        foreach (var request in requests)
        {
            var result = await EnrollAsync(
                request,
                placementOfficerUserId);

            if (result != null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    public async Task<List<StudentResponse>> GetAllAsync()
    {
        var students =
            await _studentRepository.GetAllAsync();

        var responses = new List<StudentResponse>();

        foreach (var student in students)
        {
            var response = await BuildResponseAsync(student);

            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<List<StudentResponse>> GetPendingAsync()
    {
        var students =
            await _studentRepository.GetPendingAsync();

        var responses = new List<StudentResponse>();

        foreach (var student in students)
        {
            var response = await BuildResponseAsync(student);

            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<List<StudentResponse>> GetByPlacementOfficerUserIdAsync(
        string placementOfficerUserId)
    {
        var officerUser =
            await _userManager.FindByIdAsync(placementOfficerUserId);

        if (officerUser == null)
        {
            return new List<StudentResponse>();
        }

        var officer =
            await _placementOfficerRepository.GetByEmailAsync(
                officerUser.Email ?? string.Empty);

        if (officer == null)
        {
            return new List<StudentResponse>();
        }

        var students =
            await _studentRepository.GetByCollegeIdAsync(officer.CollegeId);

        var responses = new List<StudentResponse>();

        foreach (var student in students)
        {
            var response = await BuildResponseAsync(student);

            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<List<StudentResponse>> GetPendingByPlacementOfficerUserIdAsync(
        string placementOfficerUserId)
    {
        var officerUser =
            await _userManager.FindByIdAsync(placementOfficerUserId);

        if (officerUser == null)
        {
            return new List<StudentResponse>();
        }

        var officer =
            await _placementOfficerRepository.GetByEmailAsync(
                officerUser.Email ?? string.Empty);

        if (officer == null)
        {
            return new List<StudentResponse>();
        }

        var students =
            await _studentRepository.GetPendingByCollegeIdAsync(officer.CollegeId);

        var responses = new List<StudentResponse>();

        foreach (var student in students)
        {
            var response = await BuildResponseAsync(student);

            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<StudentResponse?> GetByIdAsync(int id)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            return null;
        }

        return await BuildResponseAsync(student);
    }

    public async Task<StudentResponse?> GetByUserIdAsync(
        string userId)
    {
        var student =
            await _studentRepository.GetByUserIdAsync(userId);

        if (student == null)
        {
            return null;
        }

        return await BuildResponseAsync(student);
    }

    public async Task<bool> ApproveAsync(
        int id,
        string placementOfficerUserId)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            return false;
        }

        var officerUser =
            await _userManager.FindByIdAsync(placementOfficerUserId);

        if (officerUser == null ||
            !officerUser.IsActive)
        {
            return false;
        }

        var isPlacementOfficer =
            await _userManager.IsInRoleAsync(officerUser, "PlacementOfficer");

        if (isPlacementOfficer)
        {
            var officer =
                await _placementOfficerRepository.GetByEmailAsync(
                    officerUser.Email ?? string.Empty);

            if (officer == null ||
                officer.CollegeId != student.CollegeId)
            {
                return false;
            }
        }

        student.Status = StudentStatus.Approved;
        student.ApprovedAt = DateTime.UtcNow;

        var user =
            await _userManager.FindByIdAsync(student.UserId);

        if (user == null)
        {
            return false;
        }

        user.IsActive = true;

        var userResult =
            await _userManager.UpdateAsync(user);

        if (!userResult.Succeeded)
        {
            return false;
        }

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    public async Task<bool> RejectAsync(
        int id,
        string placementOfficerUserId)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            return false;
        }

        var officerUser =
            await _userManager.FindByIdAsync(placementOfficerUserId);

        if (officerUser == null ||
            !officerUser.IsActive)
        {
            return false;
        }

        var isPlacementOfficer =
            await _userManager.IsInRoleAsync(officerUser, "PlacementOfficer");

        if (isPlacementOfficer)
        {
            var officer =
                await _placementOfficerRepository.GetByEmailAsync(
                    officerUser.Email ?? string.Empty);

            if (officer == null ||
                officer.CollegeId != student.CollegeId)
            {
                return false;
            }
        }

        student.Status = StudentStatus.Rejected;
        student.ApprovedAt = null;

        var user =
            await _userManager.FindByIdAsync(student.UserId);

        if (user != null)
        {
            user.IsActive = false;

            var userResult =
                await _userManager.UpdateAsync(user);

            if (!userResult.Succeeded)
            {
                return false;
            }
        }

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    public async Task<bool> SuspendAsync(int id)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            return false;
        }

        student.Status = StudentStatus.Suspended;

        var user =
            await _userManager.FindByIdAsync(student.UserId);

        if (user != null)
        {
            user.IsActive = false;

            var userResult =
                await _userManager.UpdateAsync(user);

            if (!userResult.Succeeded)
            {
                return false;
            }
        }

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateStudentRequest request,
        string currentUserId,
        bool isAdmin)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            return false;
        }

        if (!isAdmin &&
            student.UserId != currentUserId)
        {
            return false;
        }

        var user =
            await _userManager.FindByIdAsync(student.UserId);

        if (user == null)
        {
            return false;
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;

        student.Department = request.Department;
        student.Degree = request.Degree;
        student.GraduationYear = request.GraduationYear;
        student.CGPA = request.CGPA;
        student.ActiveBacklogs = request.ActiveBacklogs;
        student.ResumeUrl = request.ResumeUrl;

        var userResult =
            await _userManager.UpdateAsync(user);

        if (!userResult.Succeeded)
        {
            return false;
        }

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    private async Task<StudentResponse?> BuildResponseAsync(
        Student student)
    {
        var user =
            await _userManager.FindByIdAsync(student.UserId);

        if (user == null)
        {
            return null;
        }

        var college =
            await _collegeRepository.GetByIdAsync(student.CollegeId);

        var officer =
            await _placementOfficerRepository.GetByIdAsync(
                student.PlacementOfficerId);

        return new StudentResponse
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CollegeId = student.CollegeId,
            CollegeName = college?.CollegeName ?? string.Empty,
            PlacementOfficerId = student.PlacementOfficerId,
            PlacementOfficerName = officer?.FullName ?? string.Empty,
            RegisterNumber = student.RegisterNumber,
            Department = student.Department,
            Degree = student.Degree,
            GraduationYear = student.GraduationYear,
            CGPA = student.CGPA,
            ActiveBacklogs = student.ActiveBacklogs,
            ResumeUrl = student.ResumeUrl,
            Status = student.Status,
            ApprovedAt = student.ApprovedAt,
            IsActive = user.IsActive
        };
    }
}
