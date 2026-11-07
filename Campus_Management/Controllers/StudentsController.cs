using System.Security.Claims;
using Campus_Management.DTOs.Students;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(
        IStudentService studentService)
    {
        _studentService = studentService;
    }

    [Authorize(Roles = "PlacementOfficer,Admin,SuperAdmin")]
    [HttpPost("enroll")]
    public async Task<ActionResult<StudentResponse>> Enroll(
        EnrollStudentRequest request)
    {
        var placementOfficerUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(placementOfficerUserId))
        {
            return Unauthorized();
        }

        var response =
            await _studentService.EnrollAsync(
                request,
                placementOfficerUserId);

        if (response == null)
        {
            return BadRequest(new
            {
                message =
                    "Unable to enroll student. Check the details, register number, email and placement officer."
            });
        }

        return Ok(response);
    }

    [Authorize(Roles = "PlacementOfficer,Admin,SuperAdmin")]
    [HttpPost("bulk-enroll")]
    public async Task<ActionResult<List<StudentResponse>>> BulkEnroll(
        List<EnrollStudentRequest> requests)
    {
        var placementOfficerUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(placementOfficerUserId))
        {
            return Unauthorized();
        }

        if (requests == null || requests.Count == 0)
        {
            return BadRequest(new
            {
                message = "At least one student is required."
            });
        }

        var responses =
            await _studentService.BulkEnrollAsync(
                requests,
                placementOfficerUserId);

        return Ok(responses);
    }

    [Authorize(Roles = "Admin,SuperAdmin,PlacementOfficer")]
    [HttpGet]
    public async Task<ActionResult<List<StudentResponse>>> GetAll()
    {
        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        var students = isAdmin
            ? await _studentService.GetAllAsync()
            : await _studentService.GetByPlacementOfficerUserIdAsync(currentUserId);

        return Ok(students);
    }

    [Authorize(Roles = "Admin,SuperAdmin,PlacementOfficer")]
    [HttpGet("pending")]
    public async Task<ActionResult<List<StudentResponse>>> GetPending()
    {
        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        var students = isAdmin
            ? await _studentService.GetPendingAsync()
            : await _studentService.GetPendingByPlacementOfficerUserIdAsync(currentUserId);

        return Ok(students);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponse>> GetById(int id)
    {
        var student =
            await _studentService.GetByIdAsync(id);

        if (student == null)
        {
            return NotFound(new
            {
                message = "Student not found."
            });
        }

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var isAdmin =
            User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        var isPlacementOfficer =
            User.IsInRole("PlacementOfficer");

        var isStudent =
            User.IsInRole("Student");

        if (isStudent &&
            student.UserId != currentUserId)
        {
            return Forbid();
        }

        if (!isAdmin &&
            !isPlacementOfficer &&
            !isStudent)
        {
            return Forbid();
        }

        return Ok(student);
    }

    [Authorize(Roles = "PlacementOfficer,Admin,SuperAdmin")]
    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var placementOfficerUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(placementOfficerUserId))
        {
            return Unauthorized();
        }

        var approved =
            await _studentService.ApproveAsync(
                id,
                placementOfficerUserId);

        if (!approved)
        {
            return NotFound(new
            {
                message = "Student not found or could not be approved."
            });
        }

        return NoContent();
    }

    [Authorize(Roles = "PlacementOfficer,Admin,SuperAdmin")]
    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var placementOfficerUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(placementOfficerUserId))
        {
            return Unauthorized();
        }

        var rejected =
            await _studentService.RejectAsync(
                id,
                placementOfficerUserId);

        if (!rejected)
        {
            return NotFound(new
            {
                message = "Student not found or could not be rejected."
            });
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin,SuperAdmin,PlacementOfficer")]
    [HttpPut("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id)
    {
        var suspended =
            await _studentService.SuspendAsync(id);

        if (!suspended)
        {
            return NotFound(new
            {
                message = "Student not found."
            });
        }

        return NoContent();
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateStudentRequest request)
    {
        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var isAdmin =
            User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        var isPlacementOfficer =
            User.IsInRole("PlacementOfficer");

        var isStudent =
            User.IsInRole("Student");

        if (!isAdmin &&
            !isPlacementOfficer &&
            !isStudent)
        {
            return Forbid();
        }

        var updated =
            await _studentService.UpdateAsync(
                id,
                request,
                currentUserId,
                isAdmin || isPlacementOfficer);

        if (!updated)
        {
            return Forbid();
        }

        return NoContent();
    }

    [Authorize(Roles = "Student")]
    [HttpGet("me")]
    public async Task<ActionResult<StudentResponse>> GetMe()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var student =
            await _studentService.GetByUserIdAsync(userId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "Student profile not found."
            });
        }

        return Ok(student);
    }
}
