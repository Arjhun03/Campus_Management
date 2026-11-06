using System.ComponentModel.DataAnnotations;

namespace Campus_Management.Models;

public class JobEligibleGraduationYear
{
    [Key]
    public int JobEligibleGraduationYearId { get; set; }

    [Required]
    public int JobId { get; set; }

    [Required]
    public int GraduationYear { get; set; }
}

