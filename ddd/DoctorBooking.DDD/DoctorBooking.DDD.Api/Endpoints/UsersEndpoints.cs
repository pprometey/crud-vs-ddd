using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Users.Commands.AddUserRole;
using DoctorBooking.DDD.Application.Users.Commands.RegisterUser;
using DoctorBooking.DDD.Application.Users.Commands.RemoveUserRole;
using DoctorBooking.DDD.Application.Users.Queries.GetUserByEmail;
using DoctorBooking.DDD.Application.Users.Queries.GetUserById;
using DoctorBooking.DDD.Application.Users.Queries.GetUsersPaged;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.DDD.Api.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", RegisterUserAsync)
            .WithName("RegisterUser")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with the specified role");

        group.MapGet("/", GetUsersPagedAsync)
            .WithName("GetUsersPaged")
            .WithSummary("Get paginated list of users")
            .WithDescription("Returns a paginated list of users with cursor-based pagination and dynamic sorting.");

        group.MapGet("/{userId:guid}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Get user by ID")
            .WithDescription("Returns a single user by their unique identifier");

        group.MapGet("/by-email/{email}", GetUserByEmailAsync)
            .WithName("GetUserByEmail")
            .WithSummary("Get user by email")
            .WithDescription("Returns a single user by their email address");

        group.MapPost("/{userId:guid}/roles", AddUserRoleAsync)
            .WithName("AddUserRole")
            .WithSummary("Add role to user")
            .WithDescription("Adds a new role to an existing user");

        group.MapDelete("/{userId:guid}/roles/{role}", RemoveUserRoleAsync)
            .WithName("RemoveUserRole")
            .WithSummary("Remove role from user")
            .WithDescription("Removes a role from an existing user");

        return group;
    }

    private static async Task<IResult> RegisterUserAsync(
        [FromBody] RegisterUserRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Role);
        
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Created($"/api/users/{result.Value}", new { id = result.Value });
    }

    private static async Task<IResult> GetUsersPagedAsync(
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
        var query = new GetUsersPagedQuery(request);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetUserByIdAsync(
        Guid userId,
        [FromServices] IQueryDispatcher queryDispatcher,
        CancellationToken ct)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.NotFound(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetUserByEmailAsync(
        string email,
        [FromServices] IQueryDispatcher queryDispatcher,
        CancellationToken ct)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await queryDispatcher.DispatchAsync(query, ct);

        if (result.IsFailure)
            return Results.NotFound(result.Errors);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> AddUserRoleAsync(
        Guid userId,
        [FromBody] AddRoleRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new AddUserRoleCommand(userId, request.Role);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveUserRoleAsync(
        Guid userId,
        string role,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var command = new RemoveUserRoleCommand(userId, role);
        var result = await commandDispatcher.DispatchAsync(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }
}

public sealed record RegisterUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Role);

public sealed record AddRoleRequest(string Role);
