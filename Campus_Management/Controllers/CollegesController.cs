using Campus_Management.DTOs.Colleges;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class CollegesController : ControllerBase
{
    private readonly ICollegeService _collegeService;

    public CollegesController(ICollegeService collegeService)
    {
        _collegeService = collegeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CollegeResponse>>> GetAll()
    {
        var colleges = await _collegeService.GetAllAsync();

        return Ok(colleges);
    }

    [HttpGet("{collegeId:int}")]
    public async Task<ActionResult<CollegeResponse>> GetById(int collegeId)
    {
        var college = await _collegeService.GetByIdAsync(collegeId);

        if (college == null)
        {
            return NotFound(new
            {
                message = "College not found."
            });
        }

        return Ok(college);
    }

    [HttpPost]
    public async Task<ActionResult<CollegeResponse>> Create(
        CreateCollegeRequest request)
    {
        var college = await _collegeService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { collegeId = college.CollegeId },
            college);
    }

    [HttpPut("{collegeId:int}")]
    public async Task<IActionResult> Update(
        int collegeId,
        UpdateCollegeRequest request)
    {
        var updated = await _collegeService.UpdateAsync(
            collegeId,
            request);

        if (!updated)
        {
            return NotFound(new
            {
                message = "College not found."
            });
        }

        return NoContent();
    }

    [HttpDelete("{collegeId:int}")]
    public async Task<IActionResult> Delete(int collegeId)
    {
        var deleted = await _collegeService.DeleteAsync(collegeId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "College not found."
            });
        }

        return NoContent();
    }
}
