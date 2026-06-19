using TestTask.Domain;
using TestTask.Dtos;

namespace TestTask.Service;

public interface IRequestService
{
    Task<(RequestResponseDto, bool created)> CreateAsync(
        CreateRequestDto requestDto, string? idempotencyKey, CancellationToken ct);
    
    Task<IReadOnlyList<RequestResponseDto>> GetMineAsync(CancellationToken ct);
    
    Task<IReadOnlyList<RequestResponseDto>> GetQueueAsync(RequestStatus? status, CancellationToken ct);

    Task<RequestResponseDto> GetByIdAsync(Guid id, CancellationToken ct);
    
    Task<RequestResponseDto> ChangeStatusAsync(Guid id, ChangeStatusDto dto, CancellationToken ct);
    
    Task<IReadOnlyList<StatusHistoryDto>> GetHistoryAsync(Guid id, CancellationToken ct);
}