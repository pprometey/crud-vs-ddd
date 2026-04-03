using System.Diagnostics;
using System.Net;
using Core.Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.DDD.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ConcurrencyConflictException concurrencyEx => HandleConcurrencyException(context, concurrencyEx),
            DomainException domainEx => HandleDomainException(context, domainEx),
            ArgumentException argEx => HandleValidationException(context, argEx),
            InvalidOperationException invalidOpEx => HandleValidationException(context, invalidOpEx),
            NotImplementedException => (
                HttpStatusCode.NotImplemented,
                CreateProblemDetails(context, HttpStatusCode.NotImplemented, "Not Implemented", exception.Message)
            ),
            _ => HandleUnknownException(context, exception)
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private (HttpStatusCode, ProblemDetails) HandleConcurrencyException(
        HttpContext context,
        ConcurrencyConflictException exception)
    {
        _logger.LogWarning(
            exception,
            "Concurrency conflict: {Message}",
            exception.Message);

        var problemDetails = CreateProblemDetails(
            context,
            HttpStatusCode.Conflict,
            "Concurrency Conflict",
            exception.Message);

        return (HttpStatusCode.Conflict, problemDetails);
    }

    private (HttpStatusCode, ProblemDetails) HandleDomainException(
        HttpContext context,
        DomainException exception)
    {
        _logger.LogWarning(
            exception,
            "Domain exception occurred: {ErrorCode} - {Message}",
            exception.ErrorCode,
            exception.Message);

        var problemDetails = CreateProblemDetails(
            context,
            HttpStatusCode.BadRequest,
            "Domain Rule Violation",
            exception.Message);

        problemDetails.Extensions["errorCode"] = exception.ErrorCode;

        return (HttpStatusCode.BadRequest, problemDetails);
    }

    private (HttpStatusCode, ProblemDetails) HandleValidationException(
        HttpContext context,
        Exception exception)
    {
        _logger.LogWarning(
            exception,
            "Validation exception occurred: {Message}",
            exception.Message);

        var problemDetails = CreateProblemDetails(
            context,
            HttpStatusCode.BadRequest,
            "Validation Error",
            exception.Message);

        return (HttpStatusCode.BadRequest, problemDetails);
    }

    private (HttpStatusCode, ProblemDetails) HandleUnknownException(
        HttpContext context,
        Exception exception)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred: {Message}",
            exception.Message);

        var problemDetails = CreateProblemDetails(
            context,
            HttpStatusCode.InternalServerError,
            "Internal Server Error",
            "An unexpected error occurred. Please contact support if the problem persists.");

        return (HttpStatusCode.InternalServerError, problemDetails);
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = title,
            Detail = detail,
            Status = (int)statusCode,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier
            }
        };
    }
}
