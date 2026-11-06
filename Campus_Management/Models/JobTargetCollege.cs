using System.ComponentModel.DataAnnotations;

namespace Campus_Management.Models;

public class JobTargetCollege
{
    [Key]
    public int JobTargetCollegeId { get; set; }

    [Required]
    public int JobId { get; set; }

    [Required]
    public int CollegeId { get; set; }
}