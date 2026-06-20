using Microsoft.AspNetCore.Mvc;
using TestTask.Domain;
using TestTask.Dtos;
using TestTask.Service;

namespace TestTask.Controllers;

[ApiController]
[Route("api/requests")]
[Produces("application/json")]
public class RequestsController(IRequestService requestService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRequestDto dto,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        var (request, created) = await requestService.CreateAsync(dto, idempotencyKey, ct);
        
        return created 
            ? CreatedAtAction(nameof(GetById), new { id = request.Id }, request)
            : Ok(request);
    }
    
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Ok(await requestService.GetMineAsync(ct));
    
    [HttpGet]
    public async Task<IActionResult> GetQueue([FromQuery] RequestStatus? status, CancellationToken ct)
        => Ok(await requestService.GetQueueAsync(status, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await requestService.GetByIdAsync(id, ct));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto, CancellationToken ct)
        => Ok(await requestService.ChangeStatusAsync(id, dto, ct));
    
    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
        => Ok(await requestService.GetHistoryAsync(id, ct));
}