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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? IdempotencyKey { get; set; }
    
    public long RowVersion { get; set; }

    public List<RequestStatusHistory> History { get; set; } = [];

    public void ChangeStatus(RequestStatus next, string changedBy, string? comment)
    {
        if (!StatusTransition.CanTransition(Status, next))
            throw new InvalidStatusTransitionException(Status, next);
        
        var from = Status;
        Status = next;
        RowVersion++;
    }

}