using System.Security.Claims;
using Campus_Management.DTOs.InterviewRounds;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/interview-rounds")]
[Authorize]
public class InterviewRoundsController : ControllerBase
{
    private readonly IInterviewRoundService _roundService;

    public InterviewRoundsController(IInterviewRoundService roundService)
    {
        _roundService = roundService;
    }

    [HttpPost("~/api/jobs/{jobId:int}/interview-rounds")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<ActionResult<InterviewRoundResponse>> CreateForJob(
        [FromRoute] int jobId,
        [FromBody] CreateInterviewRoundRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _roundService.CreateForJobAsync(jobId, request, userId);

        if (response == null)
        {
            return BadRequest(new
            {
                message = "Unable to create interview round. Ensure the job exists and belongs to your company, and the round number is unique."
            });
        }

        return Ok(response);
    }

    [HttpGet("~/api/jobs/{jobId:int}/interview-rounds")]
    public async Task<ActionResult<List<InterviewRoundResponse>>> GetByJobId(
        int jobId)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var responses = await _roundService.GetByJobIdAsync(
            jobId,
            currentUserId,
            role);

        if (responses == null)
        {
            return NotFound(new
            {
                message = "Job not found or access denied."
            });
        }

        return Ok(responses);
    }

    [HttpPost]
    [Authorize(Roles = "CompanyHR")]
    public async Task<ActionResult<InterviewRoundResponse>> Create(
        CreateInterviewRoundRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _roundService.CreateAsync(request, userId);

        if (response == null)
        {
            return BadRequest(new
            {
                message = "Unable to create interview round. Ensure the recruitment drive belongs to your company and the round number is unique."
            });
        }

        return Ok(response);
    }

    [HttpGet("drive/{driveId:int}")]
    public async Task<ActionResult<List<InterviewRoundResponse>>> GetByDriveId(
        int driveId)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var responses = await _roundService.GetByDriveIdAsync(
            driveId,
            currentUserId,
            role);

        return Ok(responses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InterviewRoundResponse>> GetById(int id)
    {
        var currentUserId = GetUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();

        var response = await _roundService.GetByIdAsync(
            id,
            currentUserId,
            role);

        if (response == null)
        {
            return NotFound(new
            {
                message = "Interview round not found or access denied."
            });
        }

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Update(
        int id,
        UpdateInterviewRoundRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var updated = await _roundService.UpdateAsync(
            id,
            request,
            userId);

        if (!updated)
        {
            return BadRequest(new
            {
                message = "Unable to update interview round. Ensure the recruitment drive belongs to your company and the round number is unique."
            });
        }

        return Ok(new
        {
            message = "Interview round updated successfully."
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

        var deleted = await _roundService.DeleteAsync(id, userId);

        if (!deleted)
        {
            return BadRequest(new
            {
                message = "Unable to delete interview round. Ensure the round belongs to your company."
            });
        }

        return Ok(new
        {
            message = "Interview round deleted successfully."
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
