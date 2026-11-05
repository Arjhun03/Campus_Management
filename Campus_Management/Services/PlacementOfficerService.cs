using Campus_Management.DTOs.PlacementOfficers;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;

namespace Campus_Management.Services;

public class PlacementOfficerService : IPlacementOfficerService
{
    private readonly IPlacementOfficerRepository _placementOfficerRepository;

    public PlacementOfficerService(
        IPlacementOfficerRepository placementOfficerRepository)
    {
        _placementOfficerRepository = placementOfficerRepository;
    }

    public async Task<IEnumerable<PlacementOfficerResponse>> GetAllAsync()
    {
        var placementOfficers =
            await _placementOfficerRepository.GetAllAsync();

        return placementOfficers.Select(MapToResponse);
    }

    public async Task<PlacementOfficerResponse?> GetByIdAsync(
        int placementOfficerId)
    {
        var placementOfficer =
            await _placementOfficerRepository.GetByIdAsync(
                placementOfficerId);

        return placementOfficer == null
            ? null
            : MapToResponse(placementOfficer);
    }

    public async Task<PlacementOfficerResponse> CreateAsync(
        CreatePlacementOfficerRequest request)
    {
        var placementOfficer = new PlacementOfficer
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CollegeId = request.CollegeId
        };

        var createdPlacementOfficer =
            await _placementOfficerRepository.AddAsync(
                placementOfficer);

        var createdOfficer =
            await _placementOfficerRepository.GetByIdAsync(
                createdPlacementOfficer.PlacementOfficerId);

        return MapToResponse(createdOfficer!);
    }

    public async Task<bool> UpdateAsync(
        int placementOfficerId,
        UpdatePlacementOfficerRequest request)
    {
        var placementOfficer =
            await _placementOfficerRepository.GetByIdAsync(
                placementOfficerId);

        if (placementOfficer == null)
        {
            return false;
        }

        placementOfficer.FullName = request.FullName;
        placementOfficer.Email = request.Email;
        placementOfficer.PhoneNumber = request.PhoneNumber;
        placementOfficer.CollegeId = request.CollegeId;

        await _placementOfficerRepository.UpdateAsync(
            placementOfficer);

        return true;
    }

    public async Task<bool> DeleteAsync(int placementOfficerId)
    {
        var placementOfficer =
            await _placementOfficerRepository.GetByIdAsync(
                placementOfficerId);

        if (placementOfficer == null)
        {
            return false;
        }

        await _placementOfficerRepository.DeleteAsync(
            placementOfficer);

        return true;
    }

    private static PlacementOfficerResponse MapToResponse(
        PlacementOfficer placementOfficer)
    {
        return new PlacementOfficerResponse
        {
            PlacementOfficerId = placementOfficer.PlacementOfficerId,
            FullName = placementOfficer.FullName,
            Email = placementOfficer.Email,
            PhoneNumber = placementOfficer.PhoneNumber,
            CollegeId = placementOfficer.CollegeId,
            CollegeName = placementOfficer.College?.CollegeName
                          ?? string.Empty
        };
    }
}