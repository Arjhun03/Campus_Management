using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.Applications;

public class CreateJobApplicationRequest
{
    [Required]
    public int JobId { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }
}
