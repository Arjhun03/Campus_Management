using System.Security.Claims;
using Campus_Management.DTOs.InterviewSchedules;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/interview-schedules")]
[Authorize]
public class InterviewSchedulesController : ControllerBase
{
    private readonly IInterviewScheduleService _scheduleService;

    public InterviewSchedulesController(IInterviewScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpPost]
    [Authorize(Roles = "CompanyHR")]
    public async Task<ActionResult<InterviewScheduleResponse>> Create(
        CreateInterviewScheduleRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _scheduleService.CreateAsync(request, userId);

        if (response == null)
        {
            return BadRequest(new
            {
                message = "Unable to create interview schedule. Ensure the round belongs to your company, the application is for this job and not rejected/withdrawn, timing is valid, and the applicant is not already scheduled for this round."
            });
        }

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InterviewScheduleResponse>> GetById(int id)
    {
        var currentUserId = GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();
        var response = await _scheduleService.GetByIdAsync(id, currentUserId, role);

        if (response == null)
        {
            return NotFound(new
            {
                message = "Interview schedule not found or access denied."
            });
        }

        return Ok(response);
    }

    [HttpGet("round/{roundId:int}")]
    public async Task<ActionResult<List<InterviewScheduleResponse>>> GetByRoundId(int roundId)
    {
        var currentUserId = GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();
        var responses = await _scheduleService.GetByRoundIdAsync(roundId, currentUserId, role);

        return Ok(responses);
    }

    [HttpGet("application/{applicationId:int}")]
    public async Task<ActionResult<List<InterviewScheduleResponse>>> GetByApplicationId(int applicationId)
    {
        var currentUserId = GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();
        var responses = await _scheduleService.GetByApplicationIdAsync(applicationId, currentUserId, role);

        return Ok(responses);
    }

    [HttpGet("my-schedules")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<InterviewScheduleResponse>>> GetMySchedules()
    {
        var currentUserId = GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var responses = await _scheduleService.GetMySchedulesAsync(currentUserId);

        return Ok(responses);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> Update(int id, UpdateInterviewScheduleRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var updated = await _scheduleService.UpdateAsync(id, request, userId);

        if (!updated)
        {
            return BadRequest(new
            {
                message = "Unable to update interview schedule. Verify ownership and valid timing."
            });
        }

        return Ok(new
        {
            message = "Interview schedule updated successfully."
        });
    }

    [HttpPut("{id:int}/feedback")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> SubmitFeedback(int id, InterviewFeedbackRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var updated = await _scheduleService.SubmitFeedbackAsync(id, request, userId);

        if (!updated)
        {
            return BadRequest(new
            {
                message = "Unable to submit feedback. Verify ownership."
            });
        }

        return Ok(new
        {
            message = "Interview feedback submitted successfully."
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

        var deleted = await _scheduleService.DeleteAsync(id, userId);

        if (!deleted)
        {
            return BadRequest(new
            {
                message = "Unable to delete interview schedule. Verify ownership."
            });
        }

        return Ok(new
        {
            message = "Interview schedule deleted successfully."
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
