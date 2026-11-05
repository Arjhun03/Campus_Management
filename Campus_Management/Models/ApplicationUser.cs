using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}