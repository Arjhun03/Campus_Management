using Campus_Management.Models;

namespace Campus_Management.Repositories.Interfaces;

public interface IPlacementOfficerRepository
{
    Task<IEnumerable<PlacementOfficer>> GetAllAsync();

    Task<PlacementOfficer?> GetByIdAsync(int placementOfficerId);

    Task<PlacementOfficer> AddAsync(PlacementOfficer placementOfficer);

    Task UpdateAsync(PlacementOfficer placementOfficer);

    Task DeleteAsync(PlacementOfficer placementOfficer);
}