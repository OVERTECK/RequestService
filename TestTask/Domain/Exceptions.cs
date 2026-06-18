namespace TestTask.Domain;

public abstract class DomainException(string message) : Exception(message);

public sealed class NotFoundException(string message) : DomainException(message);

public sealed class ForbiddenException(string message) : DomainException(message);

public sealed class ValidationException(string message) : DomainException(message);

public sealed class InvalidStatusTransitionException(RequestStatus from, RequestStatus to)
    : DomainException($"Невозможен переход статуса: {from} к {to}");