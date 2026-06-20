using System.ComponentModel.DataAnnotations;
using TestTask.Domain;

namespace TestTask.Dtos;

public class CreateRequestDto
{
    [Required]
    public CertificateType Type { get; set; }
    
    public string? CustomDescription { get; set; }
    
    [Range(1, 50)]
    public int CopiesCount { get; set; } = 1;
    
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = default!;
}

public class ChangeStatusDto
{
    [Required]
    public RequestStatus Status { get; set; }

    public string? Comment { get; set; }
}

public record RequestResponseDto(
    Guid Id,
    string EmployeeId,
    CertificateType Type,
    string? CustomDescription,
    int CopiesCount,
    string Reason,
    RequestStatus Status,
    DateTime CreatedAt,
    IReadOnlyCollection<RequestStatus> AllowedNextStatuses);
    
public record StatusHistoryDto(
    RequestStatus? FromStatus,
    RequestStatus ToStatus,
    string ChangedBy,
    string? Comment,
    DateTime ChangedAt);