namespace TestTask.Domain;

public static class StatusTransition
{
    private static readonly IReadOnlyDictionary<RequestStatus, RequestStatus[]> Allowed =
        new Dictionary<RequestStatus, RequestStatus[]>
        {
            [RequestStatus.New] = [RequestStatus.InProgress, RequestStatus.Rejected],
            [RequestStatus.InProgress] = [RequestStatus.Ready, RequestStatus.Rejected],
            [RequestStatus.Ready] = [RequestStatus.Issued],
            [RequestStatus.Issued] = [],
            [RequestStatus.Rejected] = [],
        };

    public static bool CanTransition(RequestStatus from, RequestStatus to)
    {
        return Allowed.TryGetValue(from, out var targets) && Array.IndexOf(targets, to) >= 0;
    }

    public static IReadOnlyCollection<RequestStatus> Next(RequestStatus from)
    {
        return Allowed.TryGetValue(from, out var targets) ? targets : [];
    }
}