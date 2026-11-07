using System.Security.Claims;
using Campus_Management.DTOs.CompanyHRs;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;


[ApiController]
[Route("api/company-hrs")]
public class CompanyHRsController : ControllerBase
{
    private readonly ICompanyHRService _companyHRService;

    public CompanyHRsController(
        ICompanyHRService companyHRService)
    {
        _companyHRService = companyHRService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<CompanyHRResponse>> Register(
        RegisterCompanyHRRequest request)
    {
        var response =
            await _companyHRService.RegisterAsync(request);

        if (response == null)
        {
            return BadRequest(new
            {
                message =
                    "Company not found or email is already registered."
            });
        }

        return Ok(response);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("pending")]
    public async Task<ActionResult<List<CompanyHRResponse>>> GetPending()
    {
        var companyHRs =
            await _companyHRService.GetPendingAsync();

        return Ok(companyHRs);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<List<CompanyHRResponse>>> GetAll()
    {
        var companyHRs =
            await _companyHRService.GetAllAsync();

        return Ok(companyHRs);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var adminUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        var approved =
            await _companyHRService.ApproveAsync(
                id,
                adminUserId);

        if (!approved)
        {
            return NotFound(new
            {
                message = "Company HR not found."
            });
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var rejected =
            await _companyHRService.RejectAsync(id);

        if (!rejected)
        {
            return NotFound(new
            {
                message = "Company HR not found."
            });
        }

        return NoContent();
    }

    [Authorize(Roles = "CompanyHR")]
    [HttpGet("me")]
    public async Task<ActionResult<CompanyHRResponse>> GetMe()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var companyHR =
            await _companyHRService.GetByUserIdAsync(userId);

        if (companyHR == null)
        {
            return NotFound(new
            {
                message = "Company HR profile not found."
            });
        }

        return Ok(companyHR);
    }
}