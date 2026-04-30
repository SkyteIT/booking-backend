using Microsoft.EntityFrameworkCore;
using Ube.Api.Middleware;
using Ube.Application.Interfaces;
using Ube.Application.Services;
using Ube.Infrastructure.Persistence;
using Ube.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// =========================
// Add framework services
// =========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseAllOfToExtendReferenceSchemas();
    options.UseInlineDefinitionsForEnums();
});

// =========================
// ✅ Add CORS (ONLY ONCE)
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // your frontend port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =========================
// Add DbContext
// =========================
builder.Services.AddDbContext<UbeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================
// Register Application services
// =========================
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<UbeDbContext>());

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();

var app = builder.Build();

// =========================
// Middleware pipeline
// =========================

// ✅ MUST be first — catches all unhandled exceptions from any middleware below
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UbeDbContext>();
    await db.Database.MigrateAsync();          // applies any pending migrations
    await DataSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();

// ✅ IMPORTANT: CORS must be here
app.UseCors("FrontendPolicy");

app.UseAuthorization();
app.MapControllers();

app.Run();