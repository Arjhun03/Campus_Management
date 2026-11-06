using System.ComponentModel.DataAnnotations;

namespace Campus_Management.Models;

public class JobEligibleDepartment
{
    [Key]
    public int JobEligibleDepartmentId { get; set; }

    [Required]
    public int JobId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;
}
