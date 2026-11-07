using System.Security.Claims;
using Campus_Management.DTOs.ApplicationStatusHistories;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Route("api/application-status-histories")]
[Authorize]
public class ApplicationStatusHistoriesController : ControllerBase
{
    private readonly IApplicationStatusHistoryService _historyService;

    public ApplicationStatusHistoriesController(IApplicationStatusHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet("~/api/applications/{applicationId:int}/history")]
    [HttpGet("application/{applicationId:int}")]
    public async Task<ActionResult<List<ApplicationStatusHistoryResponse>>> GetByApplicationId(int applicationId)
    {
        var currentUserId = GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var role = GetRole();
        var histories = await _historyService.GetByApplicationIdAsync(
            applicationId,
            currentUserId,
            role);

        if (histories == null)
        {
            return NotFound(new
            {
                message = "Application not found or access denied."
            });
        }

        return Ok(histories);
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
