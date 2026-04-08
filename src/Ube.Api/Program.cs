using Microsoft.EntityFrameworkCore;
using Ube.Infrastructure.Persistence;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Dashboard;
using Ube.Application.Features.Bookings;
using Ube.Infrastructure.Persistence.Repositories.Bookings;
using Ube.Infrastructure.Persistence.Seed;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, VendorDashboardService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await TestDataSeeder.SeedAsync(dbContext, app.Logger);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

