using System.Security.Claims;
using Campus_Management.DTOs.Jobs;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IJobTargetCollegeService _targetCollegeService;

    public JobsController(
        IJobService jobService,
        IJobTargetCollegeService targetCollegeService)
    {
        _jobService = jobService;
        _targetCollegeService = targetCollegeService;
    }

    [HttpPost]
    [Authorize(Roles = "CompanyHR")]
    public async Task<ActionResult<JobResponse>> Create(
        CreateJobRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response =
            await _jobService.CreateAsync(
                request,
                userId);

        if (response == null)
        {
            return BadRequest(
                "Unable to create the job opening.");
        }

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<JobResponse>>> GetAll()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var response =
            await _jobService.GetAllAsync(
                userId,
                role);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobResponse>> GetById(
        int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var response =
            await _jobService.GetByIdAsync(
                id,
                userId,
                role);

        if (response == null)
        {
            return NotFound(
                "Job opening not found or access denied.");
        }

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Update(
        int id,
        UpdateJobRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success =
            await _jobService.UpdateAsync(
                id,
                request,
                userId);

        if (!success)
        {
            return BadRequest(
                "Job could not be updated. Only the owning CompanyHR can update a draft job.");
        }

        return Ok(
            new
            {
                message = "Job updated successfully."
            });
    }

    [HttpPut("{id:int}/publish")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Publish(
        int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success =
            await _jobService.PublishAsync(
                id,
                userId);

        if (!success)
        {
            return BadRequest(
                "Job could not be published. If restricted to specific colleges, ensure at least one college is targeted.");
        }

        return Ok(
            new
            {
                message = "Job published successfully."
            });
    }

    [HttpPut("{id:int}/close")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Close(
        int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success =
            await _jobService.CloseAsync(
                id,
                userId);

        if (!success)
        {
            return BadRequest(
                "Only the owning CompanyHR can close a published job.");
        }

        return Ok(
            new
            {
                message = "Job closed successfully."
            });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success =
            await _jobService.DeleteAsync(
                id,
                userId);

        if (!success)
        {
            return BadRequest(
                "Only the owning CompanyHR can delete a draft job.");
        }

        return Ok(
            new
            {
                message = "Job deleted successfully."
            });
    }

    [HttpGet("eligible")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<JobResponse>>> GetEligible()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response =
            await _jobService.GetEligibleAsync(
                userId);

        return Ok(response);
    }

    [HttpGet("{id:int}/eligibility")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<JobEligibilityResponse>> GetEligibility(
        int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response =
            await _jobService.GetEligibilityAsync(
                id,
                userId);

        if (response == null)
        {
            return NotFound(
                "Job opening not found.");
        }

        return Ok(response);
    }

    [HttpPost("{id:int}/target-colleges")]
    [Authorize(Roles = "CompanyHR,Admin")]
    public async Task<ActionResult<JobTargetCollegeResponse>> AddTargetCollege(
        int id,
        TargetCollegeRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response =
            await _targetCollegeService.AddTargetAsync(
                id,
                request,
                userId);

        if (response == null)
        {
            return BadRequest(
                "Unable to target this college. Verify the job is in Draft, college exists, is approved, and is not already targeted.");
        }

        return Ok(response);
    }

    [HttpGet("{id:int}/target-colleges")]
    [Authorize(Roles = "CompanyHR,Admin")]
    public async Task<ActionResult<List<JobTargetCollegeResponse>>> GetTargetColleges(
        int id)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response =
            await _targetCollegeService.GetTargetsAsync(
                id,
                userId);

        return Ok(response);
    }

    [HttpDelete("{id:int}/target-colleges/{collegeId:int}")]
    [Authorize(Roles = "CompanyHR,Admin")]
    public async Task<IActionResult> RemoveTargetCollege(
        int id,
        int collegeId)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success =
            await _targetCollegeService.RemoveTargetAsync(
                id,
                collegeId,
                userId);

        if (!success)
        {
            return BadRequest(
                "Unable to remove the target college. Verify the job is in Draft and ownership rules are respected.");
        }

        return Ok(
            new
            {
                message = "Target college removed successfully."
            });
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }

    private string GetRole()
    {
        return User.FindFirstValue(
                   ClaimTypes.Role)
               ?? string.Empty;
    }
}
