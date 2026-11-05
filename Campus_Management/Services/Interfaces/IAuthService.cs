using Campus_Management.DTOs.Auth;

namespace Campus_Management.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginDto request);

    Task<bool> ChangePasswordAsync(
        string userId,
        ChangePasswordDto request);
}