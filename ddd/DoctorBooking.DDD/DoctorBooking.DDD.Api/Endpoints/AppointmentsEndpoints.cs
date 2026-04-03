using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Commands.AddPayment;
using DoctorBooking.DDD.Application.Appointments.Commands.BookAppointment;
using DoctorBooking.DDD.Application.Appointments.Commands.CancelAppointment;
using DoctorBooking.DDD.Application.Appointments.Commands.CompleteAppointment;
using DoctorBooking.DDD.Application.Appointments.Commands.MarkNoShow;
using DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentById;
using DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentsByDoctor;
using DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentsByPatient;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.DDD.Api.Endpoints;

public static class AppointmentsEndpoints
{
    public static RouteGroupBuilder MapAppointmentsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", BookAppointmentAsync)
            .WithName("BookAppointment")
            .WithSummary("Book a new appointment")
            .WithDescription("Creates a new appointment for a patient in a specific time slot");

        group.MapGet("/{appointmentId:guid}", GetAppointmentByIdAsync)
            .WithName("GetAppointmentById")
            .WithSummary("Get appointment by ID")
            .WithDescription("Returns a single appointment by its unique identifier");

        group.MapGet("/by-doctor/{doctorId:guid}", GetAppointmentsByDoctorAsync)
            .WithName("GetAppointmentsByDoctor")
            .WithSummary("Get appointments for a specific doctor")
            .WithDescription("Returns a paginated list of appointments for the specified doctor with cursor-based pagination and dynamic sorting.");

        group.MapGet("/by-patient/{patientId:guid}", GetAppointmentsByPatientAsync)
            .WithName("GetAppointmentsByPatient")
            .WithSummary("Get appointments for a specific patient")
            .WithDescription("Returns a paginated list of appointments for the specified patient with cursor-based pagination and dynamic sorting.");

        group.MapPost("/{appointmentId:guid}/cancel", CancelAppointmentAsync)
            .WithName("CancelAppointment")
            .WithSummary("Cancel an appointment")
            .WithDescription("Cancels an existing appointment");

        group.MapPost("/{appointmentId:guid}/complete", CompleteAppointmentAsync)
            .WithName("CompleteAppointment")
            .WithSummary("Complete an appointment")
            .WithDescription("Marks an appointment as completed");

        group.MapPost("/{appointmentId:guid}/no-show", MarkNoShowAsync)
            .WithName("MarkNoShow")
            .WithSummary("Mark appointment as no-show")
            .WithDescription("Marks an appointment as no-show when the patient doesn't arrive");

        group.MapPost("/{appointmentId:guid}/payments", AddPaymentAsync)
            .WithName("AddPayment")
            .WithSummary("Add payment to appointment")
            .WithDescription("Records a payment for an appointment");

        return group;
    }

    private static async Task<IResult> BookAppointmentAsync(
        [FromBody] BookAppointmentRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new BookAppointmentCommand(request.PatientId, request.SlotId);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Created($"/api/appointments/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> GetAppointmentByIdAsync(
        Guid appointmentId,
        [FromServices] IQueryDispatcher queryDispatcher,
        CancellationToken ct)
    {
        var query = new GetAppointmentByIdQuery(appointmentId);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.NotFound(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetAppointmentsByDoctorAsync(
        Guid doctorId,
        [FromQuery] string sortBy = "created_at",
        [FromQuery] string direction = "desc",
        [FromQuery] int pageSize = 25,
        [FromQuery] string? cursor = null,
        [FromServices] IQueryDispatcher queryDispatcher = null!,
        CancellationToken ct = default)
    {
        var sortDirection = direction.ToLowerInvariant() == "asc" 
            ? SortDirection.Asc 
            : SortDirection.Desc;

        var request = new PageRequest(pageSize, cursor, sortBy, sortDirection);
        var query = new GetAppointmentsByDoctorQuery(doctorId, request);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetAppointmentsByPatientAsync(
        Guid patientId,
        [FromQuery] string sortBy = "created_at",
        [FromQuery] string direction = "desc",
        [FromQuery] int pageSize = 25,
        [FromQuery] string? cursor = null,
        [FromServices] IQueryDispatcher queryDispatcher = null!,
        CancellationToken ct = default)
    {
        var sortDirection = direction.ToLowerInvariant() == "asc" 
            ? SortDirection.Asc 
            : SortDirection.Desc;

        var request = new PageRequest(pageSize, cursor, sortBy, sortDirection);
        var query = new GetAppointmentsByPatientQuery(patientId, request);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CancelAppointmentAsync(
        Guid appointmentId,
        [FromBody] CancelAppointmentRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new CancelAppointmentCommand(appointmentId, request.CancelledById);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

    private static async Task<IResult> CompleteAppointmentAsync(
        Guid appointmentId,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new CompleteAppointmentCommand(appointmentId);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

    private static async Task<IResult> MarkNoShowAsync(
        Guid appointmentId,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new MarkNoShowCommand(appointmentId);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

    private static async Task<IResult> AddPaymentAsync(
        Guid appointmentId,
        [FromBody] AddPaymentRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new AddPaymentCommand(appointmentId, request.Amount, request.PaidAt);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Created($"/api/appointments/{appointmentId}/payments/{result.Value}", new { id = result.Value });
    }
}

public sealed record BookAppointmentRequest(Guid PatientId, Guid SlotId);
public sealed record CancelAppointmentRequest(Guid CancelledById);
public sealed record AddPaymentRequest(decimal Amount, DateTime PaidAt);
