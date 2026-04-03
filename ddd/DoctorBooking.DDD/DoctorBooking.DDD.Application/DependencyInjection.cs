using Core.Common.Application.CQRS;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorBooking.DDD.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register Mediator with source generation from this assembly
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // Register custom CQRS dispatchers that wrap Mediator
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        return services;
    }
}
