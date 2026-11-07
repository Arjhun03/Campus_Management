using Campus_Management.DTOs.Companies;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campus_Management.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyResponse>>> GetAll()
    {
        var companies = await _companyService.GetAllAsync();

        return Ok(companies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyResponse>> GetById(int id)
    {
        var company = await _companyService.GetByIdAsync(id);

        if (company == null)
        {
            return NotFound(new
            {
                message = "Company not found."
            });
        }

        return Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create(
        CreateCompanyRequest request)
    {
        var company = await _companyService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = company.CompanyId },
            company);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCompanyRequest request)
    {
        var updated = await _companyService.UpdateAsync(
            id,
            request);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Company not found."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _companyService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Company not found."
            });
        }

        return NoContent();
    }
}