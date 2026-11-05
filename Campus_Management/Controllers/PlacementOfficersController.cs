using Campus_Management.DTOs.PlacementOfficers;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class PlacementOfficersController : ControllerBase
{
    private readonly IPlacementOfficerService _placementOfficerService;

    public PlacementOfficersController(
        IPlacementOfficerService placementOfficerService)
    {
        _placementOfficerService = placementOfficerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlacementOfficerResponse>>>
        GetAll()
    {
        var placementOfficers =
            await _placementOfficerService.GetAllAsync();

        return Ok(placementOfficers);
    }

    [HttpGet("{placementOfficerId:int}")]
    public async Task<ActionResult<PlacementOfficerResponse>>
        GetById(int placementOfficerId)
    {
        var placementOfficer =
            await _placementOfficerService.GetByIdAsync(
                placementOfficerId);

        if (placementOfficer == null)
        {
            return NotFound(new
            {
                message = "Placement officer not found."
            });
        }

        return Ok(placementOfficer);
    }

    [HttpPost]
    public async Task<ActionResult<PlacementOfficerResponse>>
        Create(CreatePlacementOfficerRequest request)
    {
        var placementOfficer =
            await _placementOfficerService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                placementOfficerId =
                    placementOfficer.PlacementOfficerId
            },
            placementOfficer);
    }

    [HttpPut("{placementOfficerId:int}")]
    public async Task<IActionResult> Update(
        int placementOfficerId,
        UpdatePlacementOfficerRequest request)
    {
        var updated =
            await _placementOfficerService.UpdateAsync(
                placementOfficerId,
                request);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Placement officer not found."
            });
        }

        return NoContent();
    }

    [HttpDelete("{placementOfficerId:int}")]
    public async Task<IActionResult> Delete(int placementOfficerId)
    {
        var deleted =
            await _placementOfficerService.DeleteAsync(
                placementOfficerId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Placement officer not found."
            });
        }

        return NoContent();
    }
}