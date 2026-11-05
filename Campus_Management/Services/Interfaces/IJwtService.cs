using Campus_Management.Models;

namespace Campus_Management.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}