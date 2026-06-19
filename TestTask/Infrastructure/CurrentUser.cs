namespace TestTask.Infrastructure;

public interface ICurrentUser
{
    string UserId { get; }
    string Role { get; }
    bool IsAccountant { get; }
    bool IsEmployee { get; }
}

public class CurrentUser : ICurrentUser
{
    public CurrentUser(IHttpContextAccessor accessor)
    {
        var request = accessor.HttpContext?.Request;
        UserId = request?.Headers["X-User-Id"].FirstOrDefault() ?? "unknown";
        Role = request?.Headers["X-Role"].FirstOrDefault() ?? "Employee";
    }
    
    public string UserId { get; }
    public string Role { get; }
    public bool IsAccountant => string.Equals(Role, "Accountant", StringComparison.OrdinalIgnoreCase);
    public bool IsEmployee => string.Equals(Role, "Employee", StringComparison.OrdinalIgnoreCase);
}