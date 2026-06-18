namespace TestTask.Domain;

public class RequestStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid RequestId { get; set; }
    
    public RequestStatus? FromStatus { get; set; }
    
    public RequestStatus ToStatus { get; set; }
    
    public string ChangedBy { get; set; } = default!;
    
    public string? Comment { get; set; }
    
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
}