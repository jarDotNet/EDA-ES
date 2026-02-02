using Microsoft.AspNetCore.DataProtection;
using Projections.Banking.Extensions;
using Projections.Banking.Postgres.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages();
builder.Services.AddDataProtection().SetApplicationName("Projections.Banking.API");

// Register Postgres database services using Entity Framework
var connectionString = builder.Configuration.GetConnectionString("ReadingModels")
    ?? throw new InvalidOperationException("ReadingModels tConnection string is not configured");

// Add Postgres Banking Reporting dependencies
builder.Services.AddBankingDependencies(connectionString);

// Register Feature Handlers
builder.Services.AddFeatureHandlers();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("🚀 Starting Reporting Banking API Service...");
    logger.LogInformation("📊 Database Connection: {ConnectionString}", connectionString.Replace("Password=password", "Password=***"));

    // Ensure database is created (in production, use proper migrations)
    logger.LogInformation("🔧 Ensuring database is created...");
    await app.Services.EnsureBankingDatabaseAsync();
    logger.LogInformation("✅ Database initialization completed");

    // Configure the HTTP request pipeline
    //if (app.Environment.IsDevelopment())
    //{
    app.UseSwagger();
    app.UseSwaggerUI();
    //}

    app.UseStaticFiles();
    app.UseRouting();

    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorPages();

    // Add a simple health check endpoint
    app.MapGet("/health", () => Results.Ok(new { 
        Status = "Healthy", 
        Service = "Reporting Banking API", 
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName
    })).WithTags("Health");

    logger.LogInformation("✅ Reporting Banking API Service started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Application terminated unexpectedly");
    throw;
}
finally
{
    logger.LogInformation("🛑 Reporting Banking API Service stopped");
}
