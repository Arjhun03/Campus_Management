using System.Security.Claims;
using Campus_Management.DTOs.RecruitmentDrives;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/recruitment-drives")]
[Authorize]
public class RecruitmentDrivesController : ControllerBase
{
    private readonly IRecruitmentDriveService _driveService;

    public RecruitmentDrivesController(
        IRecruitmentDriveService driveService)
    {
        _driveService = driveService;
    }

    [HttpPost]
    [Authorize(Roles = "CompanyHR")]
    public async Task<ActionResult<RecruitmentDriveResponse>> Create(
        CreateRecruitmentDriveRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response =
            await _driveService.CreateAsync(
                request,
                userId);

        if (response == null)
        {
            return BadRequest(new
            {
                message = "Unable to create recruitment drive. Ensure the job exists and belongs to your company."
            });
        }

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<RecruitmentDriveResponse>>> GetAll()
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var responses =
            await _driveService.GetAllAsync(
                currentUserId,
                role);

        return Ok(responses);
    }

    [HttpGet("job/{jobId:int}")]
    public async Task<ActionResult<List<RecruitmentDriveResponse>>> GetByJobId(
        int jobId)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var responses =
            await _driveService.GetByJobIdAsync(
                jobId,
                currentUserId,
                role);

        return Ok(responses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecruitmentDriveResponse>> GetById(int id)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var response =
            await _driveService.GetByIdAsync(
                id,
                currentUserId,
                role);

        if (response == null)
        {
            return NotFound(new
            {
                message = "Recruitment drive not found or access denied."
            });
        }

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Update(
        int id,
        UpdateRecruitmentDriveRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var updated =
            await _driveService.UpdateAsync(
                id,
                request,
                userId);

        if (!updated)
        {
            return BadRequest(new
            {
                message = "Unable to update recruitment drive. Verify ownership."
            });
        }

        return Ok(new
        {
            message = "Recruitment drive updated successfully."
        });
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var cancelled =
            await _driveService.CancelAsync(
                id,
                userId);

        if (!cancelled)
        {
            return BadRequest(new
            {
                message = "Unable to cancel recruitment drive. Verify ownership and that the drive is not already completed or cancelled."
            });
        }

        return Ok(new
        {
            message = "Recruitment drive cancelled successfully."
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var deleted =
            await _driveService.DeleteAsync(
                id,
                userId);

        if (!deleted)
        {
            return BadRequest(new
            {
                message = "Unable to delete recruitment drive. Only scheduled or cancelled drives belonging to your company can be deleted."
            });
        }

        return Ok(new
        {
            message = "Recruitment drive deleted successfully."
        });
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private string GetRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
