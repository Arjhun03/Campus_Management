using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.Companies;

public class CompanyResponse
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CompanyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}