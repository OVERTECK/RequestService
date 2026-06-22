namespace TestTask.Domain;

public class RequestStatusHistory
{
    public Guid Id { get; set; }
    
    public Guid RequestId { get; set; }
    
    public RequestStatus? FromStatus { get; set; }
    
    public RequestStatus ToStatus { get; set; }
    
    public string ChangedBy { get; set; } = default!;
    
    public string? Comment { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}