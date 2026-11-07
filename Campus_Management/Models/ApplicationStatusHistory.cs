using System.ComponentModel.DataAnnotations;
using Campus_Management.Models.Enums;

namespace Campus_Management.Models;

public class ApplicationStatusHistory
{
    [Key]
    public int HistoryId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    public ApplicationStatus? FromStatus { get; set; }

    [Required]
    public ApplicationStatus ToStatus { get; set; }

    [Required]
    public string ChangedByUserId { get; set; } = string.Empty;

    [Required]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Remarks { get; set; }
}
