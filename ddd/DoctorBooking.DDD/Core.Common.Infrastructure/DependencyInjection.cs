using Core.Common.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Common.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
