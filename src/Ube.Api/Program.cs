using Microsoft.EntityFrameworkCore;
using Ube.Infrastructure.Persistence;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Dashboard;
using Ube.Application.Features.Bookings;
using Ube.Infrastructure.Persistence.Repositories.Bookings;
using Ube.Infrastructure.Persistence.Repositories.Listings;
using Ube.Infrastructure.Persistence.Seed;
using System.Text.Json.Serialization;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Features.Listings;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
// Add FluentValidation 
builder.Services.AddFluentValidationAutoValidation();
// Register validators from the assembly containing Program
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IBlockedDateRepository, BlockedDateRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, VendorDashboardService>();
builder.Services.AddScoped<IListingService, ListingService>();
// Register availability strategies
builder.Services.AddScoped<IAvailabilityStrategy, CapacityStrategy>();
builder.Services.AddScoped<IAvailabilityStrategy, SingleUnitStrategy>();
builder.Services.AddScoped<StrategySelector>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();


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
app.UseMiddleware<Ube.Api.Middleware.ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();


app.MapControllers();

app.Run();

