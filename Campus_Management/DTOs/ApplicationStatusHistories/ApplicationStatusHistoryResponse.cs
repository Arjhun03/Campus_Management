using Campus_Management.Models.Enums;

namespace Campus_Management.DTOs.ApplicationStatusHistories;

public class ApplicationStatusHistoryResponse
{
    public int HistoryId { get; set; }

    public int ApplicationId { get; set; }

    public ApplicationStatus? FromStatus { get; set; }

    public string? FromStatusText { get; set; }

    public ApplicationStatus ToStatus { get; set; }

    public string ToStatusText { get; set; } = string.Empty;

    public string ChangedByUserId { get; set; } = string.Empty;

    public string ChangedByName { get; set; } = string.Empty;

    public string ChangedByRole { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? Remarks { get; set; }
}
