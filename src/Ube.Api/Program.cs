using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Ube.Application.Interfaces.Repositories;
using Ube.Application.Services.Cart;
using Ube.Infrastructure.Persistence;
using Ube.Infrastructure.Persistence.Repositories;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ube API",
        Version = "v1"
    });
});


builder.Services.AddScoped<ICartRepository, CartRepository>();




// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json");

/*
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
*/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));







// Add Application Services
builder.Services.AddScoped<ICartService, CartService>();

// Add Controllers
builder.Services.AddControllers();



var app = builder.Build();



// In Program.cs or your exception middleware — temporarily
app.UseExceptionHandler(app => app.Run(async context =>
{
    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    await context.Response.WriteAsJsonAsync(new 
    { 
        message    = ex?.Message,
        inner      = ex?.InnerException?.Message,
        innerInner = ex?.InnerException?.InnerException?.Message,
        stackTrace = ex?.StackTrace  // remove after debugging
    });
}));




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
{
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ube.Api v1");
});

}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();