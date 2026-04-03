using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Application.Appointments.Queries;
using DoctorBooking.DDD.Application.Schedules.Queries;
using DoctorBooking.DDD.Application.Users.Queries;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Services;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Persistence;
using DoctorBooking.DDD.Infrastructure.Persistence.QueryRepositories;
using DoctorBooking.DDD.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorBooking.DDD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Domain repositories (tracked)
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IScheduleRepository, EfScheduleRepository>();
        services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();

        // Query repositories (no tracking, DTO projection)
        services.AddScoped<IUserQueryRepository, EfUserQueryRepository>();
        services.AddScoped<IScheduleQueryRepository, EfScheduleQueryRepository>();
        services.AddScoped<IAppointmentQueryRepository, EfAppointmentQueryRepository>();

        // Domain services
        services.AddScoped<AppointmentBookingService>();
        services.AddScoped<SlotCancellationPolicy>();

        return services;
    }
}
