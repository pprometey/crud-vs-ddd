using DoctorBooking.CRUD.Db;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Register EF Core InMemory DB
builder.Services.AddDbContext<MedicalBookingContext>(options =>
    options.UseInMemoryDatabase("MedicalBookingDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MedicalBookingContext>();

    // Always recreate in-memory DB
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    // Seed test data
    DbSeeder.Seed(context);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
