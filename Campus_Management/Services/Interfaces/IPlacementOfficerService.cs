using Campus_Management.DTOs.PlacementOfficers;

namespace Campus_Management.Services.Interfaces;

public interface IPlacementOfficerService
{
    Task<IEnumerable<PlacementOfficerResponse>> GetAllAsync();

    Task<PlacementOfficerResponse?> GetByIdAsync(int placementOfficerId);

    Task<PlacementOfficerResponse> CreateAsync(
        CreatePlacementOfficerRequest request);

    Task<bool> UpdateAsync(
        int placementOfficerId,
        UpdatePlacementOfficerRequest request);

    Task<bool> DeleteAsync(int placementOfficerId);
}