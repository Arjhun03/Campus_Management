using System.ComponentModel.DataAnnotations;

namespace Campus_Management.DTOs.Jobs;

public class TargetCollegeRequest
{
    [Required]
    public int CollegeId { get; set; }
}