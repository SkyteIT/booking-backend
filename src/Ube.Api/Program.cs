using Microsoft.EntityFrameworkCore;
using Ube.Application.Interfaces;
using Ube.Application.Services;
using Ube.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- REGISTER YOUR INTERFACES & IMPLEMENTATIONS ---
// Register your DbContext
builder.Services.AddDbContext<UbeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IAppDbContext so it can be injected
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<UbeDbContext>());

// Register CategoryService for ICategoryService
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();