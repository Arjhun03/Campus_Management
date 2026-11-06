using System.Security.Claims;
using Campus_Management.DTOs.Applications;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/job-applications")]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _applicationService;

    public JobApplicationsController(
        IJobApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<JobApplicationResponse>> Apply(
        CreateJobApplicationRequest request)
    {
        var studentUserId = GetUserId();

        if (studentUserId == null)
        {
            return Unauthorized();
        }

        var response =
            await _applicationService.ApplyAsync(
                request,
                studentUserId);

        if (response == null)
        {
            return BadRequest(new
            {
                message =
                    "Unable to apply. Ensure the job is published, deadline has not passed, you are eligible, and you have not already applied."
            });
        }

        return Ok(response);
    }

    [HttpGet("my-applications")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<JobApplicationResponse>>> GetMyApplications()
    {
        var studentUserId = GetUserId();

        if (studentUserId == null)
        {
            return Unauthorized();
        }

        var response =
            await _applicationService.GetMyApplicationsAsync(
                studentUserId);

        return Ok(response);
    }

    [HttpGet("job/{jobId:int}")]
    [Authorize(Roles = "CompanyHR,PlacementOfficer,Admin")]
    public async Task<ActionResult<List<JobApplicationResponse>>> GetApplicationsByJob(
        int jobId)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var response =
            await _applicationService.GetApplicationsByJobAsync(
                jobId,
                currentUserId,
                role);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobApplicationResponse>> GetById(int id)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var response =
            await _applicationService.GetByIdAsync(
                id,
                currentUserId,
                role);

        if (response == null)
        {
            return NotFound(new
            {
                message = "Job application not found or access denied."
            });
        }

        return Ok(response);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "CompanyHR,Admin")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateApplicationStatusRequest request)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var updated =
            await _applicationService.UpdateStatusAsync(
                id,
                request,
                currentUserId,
                role);

        if (!updated)
        {
            return BadRequest(new
            {
                message =
                    "Unable to update status. Verify ownership and that the application has not been withdrawn."
            });
        }

        return Ok(new
        {
            message = "Application status updated successfully."
        });
    }

    [HttpPut("{id:int}/withdraw")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Withdraw(int id)
    {
        var studentUserId = GetUserId();

        if (studentUserId == null)
        {
            return Unauthorized();
        }

        var withdrawn =
            await _applicationService.WithdrawAsync(
                id,
                studentUserId);

        if (!withdrawn)
        {
            return BadRequest(new
            {
                message =
                    "Unable to withdraw application. Verify ownership and current status."
            });
        }

        return Ok(new
        {
            message = "Application withdrawn successfully."
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
