using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Schedules.Commands.AddSlot;
using DoctorBooking.DDD.Application.Schedules.Commands.CreateSchedule;
using DoctorBooking.DDD.Application.Schedules.Commands.RemoveSlot;
using DoctorBooking.DDD.Application.Schedules.Queries.GetScheduleByDoctor;
using DoctorBooking.DDD.Application.Schedules.Queries.GetSlotById;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.DDD.Api.Endpoints;

public static class SchedulesEndpoints
{
    public static RouteGroupBuilder MapSchedulesEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateScheduleAsync)
            .WithName("CreateSchedule")
            .WithSummary("Create a schedule for a doctor")
            .WithDescription("Creates a new schedule for a doctor");

        group.MapGet("/by-doctor/{doctorId:guid}", GetScheduleByDoctorAsync)
            .WithName("GetScheduleByDoctor")
            .WithSummary("Get schedule for a doctor")
            .WithDescription("Returns the schedule for a specific doctor");

        group.MapPost("/slots", AddSlotAsync)
            .WithName("AddSlot")
            .WithSummary("Add a time slot to schedule")
            .WithDescription("Adds a new time slot to a doctor's schedule");

        group.MapGet("/slots/{slotId:guid}", GetSlotByIdAsync)
            .WithName("GetSlotById")
            .WithSummary("Get time slot by ID")
            .WithDescription("Returns a specific time slot by its unique identifier");

        group.MapDelete("/slots", RemoveSlotAsync)
            .WithName("RemoveSlot")
            .WithSummary("Remove time slot from schedule")
            .WithDescription("Removes a time slot from a doctor's schedule");

        return group;
    }

    private static async Task<IResult> CreateScheduleAsync(
        [FromBody] CreateScheduleRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new CreateScheduleCommand(request.DoctorId);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Created($"/api/schedules/by-doctor/{request.DoctorId}", new { id = result.Value });
    }

    private static async Task<IResult> GetScheduleByDoctorAsync(
        Guid doctorId,
        [FromServices] IQueryDispatcher queryDispatcher,
        CancellationToken ct)
    {
        var query = new GetScheduleByDoctorQuery(doctorId);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.NotFound(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> AddSlotAsync(
        [FromBody] AddSlotRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new AddSlotCommand(request.DoctorId, request.Start, request.Duration, request.Price);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Created($"/api/schedules/slots/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> GetSlotByIdAsync(
        Guid slotId,
        [FromServices] IQueryDispatcher queryDispatcher,
        CancellationToken ct)
    {
        var query = new GetSlotByIdQuery(slotId);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.NotFound(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> RemoveSlotAsync(
        [FromBody] RemoveSlotRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new RemoveSlotCommand(request.DoctorId, request.SlotId);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }
}

public sealed record CreateScheduleRequest(Guid DoctorId);
public sealed record AddSlotRequest(Guid DoctorId, DateTime Start, TimeSpan Duration, decimal Price);
public sealed record RemoveSlotRequest(Guid DoctorId, Guid SlotId);
