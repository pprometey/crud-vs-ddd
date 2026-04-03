using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Common.Application.CQRS;
using Core.Common.Infrastructure;
using DoctorBooking.DDD.Api.Endpoints;
using DoctorBooking.DDD.Api.Middleware;
using DoctorBooking.DDD.Application;
using DoctorBooking.DDD.Infrastructure;
using DoctorBooking.DDD.Infrastructure.Mapped;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Infrastructure layer (clock)
builder.Services.AddInfrastructure();

// Persistence layer (EF Core, repositories, UoW)
// Переключение между Direct (domain = persistence) и Mapped (с отдельными DbModels)
var persistenceStrategy = builder.Configuration["PersistenceStrategy"];
if (persistenceStrategy == "Mapped")
{
    Console.WriteLine("Using MAPPED persistence approach (separate DbModels with mapping)");
    builder.Services.AddPersistenceMapped(builder.Configuration);
}
else
{
    Console.WriteLine("Using DIRECT persistence approach (domain models as EF entities)");
    builder.Services.AddPersistence(builder.Configuration);
}

// Application layer (CQRS handlers and dispatchers)
builder.Services.AddApplication();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Api-Version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// OpenAPI with Scalar UI
builder.Services.AddOpenApi();

// CORS for production
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        
        if (allowedOrigins?.Length > 0)
        {
            // Production: use configured origins
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Development: allow all
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    var permitLimit = builder.Configuration.GetValue<int>("RateLimiting:Fixed:PermitLimit");
    var windowMinutes = builder.Configuration.GetValue<int>("RateLimiting:Fixed:WindowMinutes");
    var queueLimit = builder.Configuration.GetValue<int>("RateLimiting:Fixed:QueueLimit");

    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = permitLimit;
        limiterOptions.Window = TimeSpan.FromMinutes(windowMinutes);
        limiterOptions.QueueLimit = queueLimit;
    });
});

var app = builder.Build();

// Auto-apply migrations and seed database on startup
using (var scope = app.Services.CreateScope())
{
    if (persistenceStrategy == "Mapped")
    {
        var db = scope.ServiceProvider.GetRequiredService<DoctorBooking.DDD.Infrastructure.Mapped.Persistence.AppDbContext>();
        await db.Database.MigrateAsync();
        DoctorBooking.DDD.Infrastructure.Mapped.Persistence.DbSeeder.Seed(db);
    }
    else
    {
        var db = scope.ServiceProvider.GetRequiredService<DoctorBooking.DDD.Infrastructure.Persistence.AppDbContext>();
        await db.Database.MigrateAsync();
        DoctorBooking.DDD.Infrastructure.Persistence.DbSeeder.Seed(db);
    }
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseResponseCompression();
app.UseCors();
app.UseRateLimiter();

// OpenAPI UI (available in all environments, but consider authentication in production)
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("DoctorBooking DDD API")
        .WithTheme(Scalar.AspNetCore.ScalarTheme.Purple)
        .WithDefaultHttpClient(Scalar.AspNetCore.ScalarTarget.CSharp, Scalar.AspNetCore.ScalarClient.HttpClient);
});

app.MapDefaultEndpoints();

// API version sets
ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

// Map API endpoints with versioning
var v1 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

v1.MapGroup("/users").MapUsersEndpoints();
v1.MapGroup("/appointments").MapAppointmentsEndpoints();
v1.MapGroup("/schedules").MapSchedulesEndpoints();

app.UseFileServer();

await app.RunAsync();
