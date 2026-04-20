using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Ube.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS (IMPORTANT for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // React default
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Controllers + Fix JSON Cycle Issue
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS (MUST be before MapControllers)
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Test API
app.MapGet("/weatherforecast", () =>
{
    return "API is running...";
});

app.Run();