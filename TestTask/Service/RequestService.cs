using Microsoft.EntityFrameworkCore;
using TestTask.Data;
using TestTask.Domain;
using TestTask.Dtos;
using TestTask.Infrastructure;

namespace TestTask.Service;

public class RequestService(AppDbContext dbContext, ICurrentUser currentUser) : IRequestService
{
    private static RequestResponseDto Map(CertificateRequest r) => new(
        r.Id, r.EmployerId, r.Type, r.CustomDescription, r.CopiesCount,
        r.Reason, r.Status, r.CreatedAt, StatusTransition.Next(r.Status));
    
    private void RequireAccountant()
    {
        if (!currentUser.IsAccountant)
            throw new ForbiddenException("Действие доступно только бухгалтеру.");
    }
    
    public async Task<(RequestResponseDto, bool created)> CreateAsync(
        CreateRequestDto requestDto, 
        string? idempotencyKey, 
        CancellationToken ct)
    {
        if (requestDto.Type == CertificateType.Custom && string.IsNullOrWhiteSpace(requestDto.CustomDescription))
            throw new ValidationException("Для произвольной справки укажите CustomDescription.");
        
        var key = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey;
        
        if (key is not null)
        {
            var existing = await dbContext.Requests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdempotencyKey == key, ct);
            if (existing is not null)
                return (Map(existing), created: false);
        }
        
        var request = new CertificateRequest
        {
            EmployerId = currentUser.UserId,
            Type = requestDto.Type,
            CustomDescription = requestDto.Type == CertificateType.Custom ? requestDto.CustomDescription : null,
            CopiesCount = requestDto.CopiesCount,
            Reason = requestDto.Reason,
            IdempotencyKey = key
        };
        
        request.History.Add(new RequestStatusHistory
        {
            RequestId = request.Id,
            FromStatus = null,
            ToStatus = RequestStatus.New,
            ChangedBy = currentUser.UserId,
            Comment = "Заявка создана"
        });

        dbContext.Requests.Add(request);
        
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (key is not null)
        {
            dbContext.ChangeTracker.Clear();
            var existing = await dbContext.Requests.AsNoTracking()
                .FirstAsync(r => r.IdempotencyKey == key, ct);
            return (Map(existing), created: false);
        }

        return (Map(request), created: true);
    }

    public async Task<IReadOnlyList<RequestResponseDto>> GetMineAsync(CancellationToken ct)
    {
        var items = await dbContext.Requests.AsNoTracking()
            .Where(r => r.EmployerId == currentUser.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<RequestResponseDto>> GetQueueAsync(RequestStatus? status, CancellationToken ct)
    {
        RequireAccountant();

        var query = dbContext.Requests.AsNoTracking().AsQueryable();
        
        if (status is not null)
            query = query.Where(r => r.Status == status);

        var items = await query.OrderBy(r => r.CreatedAt).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<RequestResponseDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var request = await dbContext.Requests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct)
                      ?? throw new NotFoundException($"Заявка {id} не найдена.");
        
        if (!currentUser.IsAccountant)
            throw new ForbiddenException("Нет доступа к этой заявке.");

        return Map(request);
    }

    public async Task<RequestResponseDto> ChangeStatusAsync(Guid id, ChangeStatusDto dto, CancellationToken ct)
    {
        RequireAccountant();

        var request = await dbContext.Requests.FirstOrDefaultAsync(r => r.Id == id, ct)
                      ?? throw new NotFoundException($"Заявка {id} не найдена.");
        
        request.ChangeStatus(dto.Status, currentUser.UserId, dto.Comment);
        
        var from = request.Status;
        
        dbContext.History.Add(new RequestStatusHistory
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            FromStatus = from,
            ToStatus = dto.Status,
            ChangedBy = currentUser.UserId,
            Comment = dto.Comment,
            ChangedAt = DateTime.UtcNow
        });
        
        await dbContext.SaveChangesAsync(ct);

        return Map(request);
    }

    public async Task<IReadOnlyList<StatusHistoryDto>> GetHistoryAsync(Guid id, CancellationToken ct)
    {
        if (!await dbContext.Requests.AnyAsync(r => r.Id == id, ct))
            throw new NotFoundException($"Заявка {id} не найдена.");

        var items = await dbContext.History.AsNoTracking()
            .Where(h => h.RequestId == id)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(ct);

        return items
            .Select(h => new StatusHistoryDto(h.FromStatus, h.ToStatus, h.ChangedBy, h.Comment, h.ChangedAt))
            .ToList();
    }
}