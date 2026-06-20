using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestTask.Domain;

namespace TestTask.Infrastructure;

public class ExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Не найдено"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Доступ запрещен"),
            ValidationException => (StatusCodes.Status422UnprocessableEntity, "Ошибка валидации"),
            InvalidStatusTransitionException => (StatusCodes.Status409Conflict, "Конфликт состояния"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                "Заявка была изменена другим пользователем"),
            _ => (StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера")
        };
        
        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Необработанная ошибка");
        
        httpContext.Response.StatusCode = status;
        
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Произошла непредвиденная ошибка."
                    : exception.Message
            }
        });
    }
}