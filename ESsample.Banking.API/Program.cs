using ESsample.Banking.API.Infrastructure.Converters;
using ESsample.Banking.API.Infrastructure.Extensions;
using EventBus.Kafka;
using EventBus.Kafka.Extensions;
using EventSourcing.Postgres.Extensions;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages();
builder.Services.AddDataProtection().SetApplicationName("ESsample.Banking.API");

// Register Event Sourcing services using Entity Framework
var connectionString = builder.Configuration.GetConnectionString("EventStore")
    ?? throw new InvalidOperationException("EventStore Connection string is not configured");

// Add Postgres EventStore
builder.Services.AddEventStore(connectionString);

// Add Kafka EventBus
var kafkaBootstrapServers = builder.Configuration.GetConnectionString("Kafka")
    ?? builder.Configuration["Kafka:BootstrapServers"]
    ?? "localhost:9092";

var topicPrefix = builder.Configuration["Kafka:TopicPrefix"]
    ?? Environment.GetEnvironmentVariable("Kafka__TopicPrefix")
    ?? "banking-events";

var kafkaConfiguration = new KafkaConfiguration
{
    BootstrapServers = kafkaBootstrapServers,
    TopicPrefix = topicPrefix
};

builder.Services.AddKafkaEventBus(kafkaConfiguration);

// Register Feature Handlers
builder.Services.AddFeatureHandlers();

// Configure JSON serialization options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new SystemTypeJsonConverter());
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("🚀 Starting Banking API Service...");
    logger.LogInformation("📊 Database Connection: {ConnectionString}", connectionString.Replace("Password=password", "Password=***"));
    logger.LogInformation("📡 Kafka Bootstrap Servers: {BootstrapServers}", kafkaBootstrapServers);
    logger.LogInformation("📋 Topic Prefix: {TopicPrefix}", topicPrefix);

    // Ensure database is created (in production, use proper migrations)
    logger.LogInformation("🔧 Ensuring database is created...");
    await app.Services.EnsureEventStoreDatabaseAsync();
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

    // Map feature endpoints
    app.MapAccountFeatures();

    app.MapControllers();
    app.MapRazorPages();

    // Add a simple health check endpoint
    app.MapGet("/health", () => Results.Ok(new { 
        Status = "Healthy", 
        Service = "Banking API", 
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName
    })).WithTags("Health");

    logger.LogInformation("✅ Banking API Service started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Application terminated unexpectedly");
    throw;
}
finally
{
    logger.LogInformation("🛑 Banking API Service stopped");
}
