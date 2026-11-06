using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.Applications;

public class UpdateApplicationStatusRequest
{
    [Required]
    public ApplicationStatus Status { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}
