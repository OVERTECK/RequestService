namespace TestTask.Domain;

public class CertificateRequest
{
    public Guid Id { get; set; }

    public string EmployerId { get; set; } = default!;
    
    public CertificateType Type { get; set; }
    
    public string? CustomDescription { get; set; }
    
    public int CopiesCount { get; set; }

    public string Reason { get; set; } = default!;
    
    public RequestStatus Status { get; private set; } = RequestStatus.New;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    
    public string? IdempotencyKey { get; set; }
    
    public Guid RowVersion { get; set; }  = Guid.NewGuid();

    public List<RequestStatusHistory> History { get; set; } = [];

    public void ChangeStatus(RequestStatus next, string changedBy, string? comment)
    {
        if (!StatusTransition.CanTransition(Status, next))
            throw new InvalidStatusTransitionException(Status, next);
        
        var from = Status;
        Status = next;
        RowVersion = Guid.NewGuid();
        
        History.Add(new RequestStatusHistory
        {
            RequestId = Id,
            FromStatus = from,
            ToStatus = next,
            ChangedBy = changedBy,
            Comment = comment,
            ChangedAt = DateTimeOffset.UtcNow
        });
    }

}