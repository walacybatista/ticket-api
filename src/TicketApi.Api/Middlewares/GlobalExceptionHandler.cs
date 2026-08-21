using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TicketApi.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro não tratado ao processar {Path}", httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception) =>
        exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),

            ArgumentException argumentEx => (
                StatusCodes.Status400BadRequest,
                "Requisição inválida",
                argumentEx.Message),

            InvalidOperationException invalidOpEx => (
                StatusCodes.Status409Conflict,
                "Operação inválida para o estado atual",
                invalidOpEx.Message),

            KeyNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                notFoundEx.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno no servidor",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };
}