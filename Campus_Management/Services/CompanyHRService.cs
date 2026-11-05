using Campus_Management.DTOs.CompanyHRs;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Campus_Management.Services;

public class CompanyHRService : ICompanyHRService
{
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICompanyRepository _companyRepository;

    public CompanyHRService(
        ICompanyHRRepository companyHRRepository,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ICompanyRepository companyRepository)
    {
        _companyHRRepository = companyHRRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _companyRepository = companyRepository;
    }

    public async Task<CompanyHRResponse?> RegisterAsync(
        RegisterCompanyHRRequest request)
    {
        var company = await _companyRepository
            .GetByIdAsync(request.CompanyId);

        if (company == null)
        {
            return null;
        }

        var existingUser = await _userManager
            .FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsActive = false,
            EmailConfirmed = false
        };

        var createResult = await _userManager
            .CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                createResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to create Company HR user: {errors}");
        }

        const string companyHRRole = "CompanyHR";

        if (!await _roleManager.RoleExistsAsync(companyHRRole))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(companyHRRole));
        }

        await _userManager.AddToRoleAsync(
            user,
            companyHRRole);

        var companyHR = new CompanyHR
        {
            UserId = user.Id,
            CompanyId = request.CompanyId,
            Designation = request.Designation,
            ApprovedBy = null,
            ApprovedAt = null
        };

        var createdCompanyHR =
            await _companyHRRepository.CreateAsync(companyHR);

        return await BuildResponseAsync(
            createdCompanyHR,
            user,
            company.CompanyName);
    }

    public async Task<List<CompanyHRResponse>> GetAllAsync()
    {
        var companyHRs =
            await _companyHRRepository.GetAllAsync();

        var responses = new List<CompanyHRResponse>();

        foreach (var companyHR in companyHRs)
        {
            var response = await BuildResponseAsync(companyHR);

            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<List<CompanyHRResponse>> GetPendingAsync()
    {
        var companyHRs =
            await _companyHRRepository.GetPendingAsync();

        var responses = new List<CompanyHRResponse>();

        foreach (var companyHR in companyHRs)
        {
            var user = await _userManager
                .FindByIdAsync(companyHR.UserId);

            if (user == null || user.IsActive)
            {
                continue;
            }

            var response = await BuildResponseAsync(
                companyHR,
                user);

            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<CompanyHRResponse?> GetByIdAsync(int id)
    {
        var companyHR =
            await _companyHRRepository.GetByIdAsync(id);

        if (companyHR == null)
        {
            return null;
        }

        return await BuildResponseAsync(companyHR);
    }

    public async Task<CompanyHRResponse?> GetByUserIdAsync(
        string userId)
    {
        var companyHR =
            await _companyHRRepository.GetByUserIdAsync(userId);

        if (companyHR == null)
        {
            return null;
        }

        return await BuildResponseAsync(companyHR);
    }

    public async Task<bool> ApproveAsync(
        int id,
        string approvedByUserId)
    {
        var companyHR =
            await _companyHRRepository.GetByIdAsync(id);

        if (companyHR == null)
        {
            return false;
        }

        var user = await _userManager
            .FindByIdAsync(companyHR.UserId);

        if (user == null)
        {
            return false;
        }

        user.IsActive = true;

        var updateUserResult =
            await _userManager.UpdateAsync(user);

        if (!updateUserResult.Succeeded)
        {
            return false;
        }

        companyHR.ApprovedBy = approvedByUserId;
        companyHR.ApprovedAt = DateTime.UtcNow;

        return await _companyHRRepository
            .UpdateAsync(companyHR);
    }

    public async Task<bool> RejectAsync(int id)
    {
        var companyHR =
            await _companyHRRepository.GetByIdAsync(id);

        if (companyHR == null)
        {
            return false;
        }

        var user = await _userManager
            .FindByIdAsync(companyHR.UserId);

        if (user == null)
        {
            return false;
        }

        user.IsActive = false;

        var updateResult =
            await _userManager.UpdateAsync(user);

        return updateResult.Succeeded;
    }

    private async Task<CompanyHRResponse?> BuildResponseAsync(
        CompanyHR companyHR,
        ApplicationUser? user = null,
        string? companyName = null)
    {
        user ??= await _userManager
            .FindByIdAsync(companyHR.UserId);

        if (user == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(companyName))
        {
            var company = await _companyRepository
                .GetByIdAsync(companyHR.CompanyId);

            companyName = company?.CompanyName ?? string.Empty;
        }

        return new CompanyHRResponse
        {
            CompanyHRId = companyHR.CompanyHRId,
            UserId = companyHR.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CompanyId = companyHR.CompanyId,
            CompanyName = companyName,
            Designation = companyHR.Designation,
            IsActive = user.IsActive,
            ApprovedBy = companyHR.ApprovedBy,
            ApprovedAt = companyHR.ApprovedAt
        };
    }
}