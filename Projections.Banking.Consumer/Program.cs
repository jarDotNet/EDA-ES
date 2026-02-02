using EventBus.Extensions;
using EventBus.Kafka;
using EventBus.Kafka.Extensions;
using Projections.Banking.Consumer.Services;
using Projections.Banking.Extensions;
using Projections.Banking.Postgres.Extensions;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Get Kafka configuration
var kafkaBootstrapServers = builder.Configuration.GetConnectionString("Kafka")
    ?? Environment.GetEnvironmentVariable("Kafka__BootstrapServers")
    ?? "localhost:9092";

var topicPrefix = builder.Configuration["Kafka:TopicPrefix"]
    ?? Environment.GetEnvironmentVariable("Kafka__TopicPrefix")
    ?? "banking-events";

var groupId = nameof(BankingBackgroundService);

var kafkaConfiguration = new KafkaConfiguration
{
    BootstrapServers = kafkaBootstrapServers,
    TopicPrefix = topicPrefix,
    GroupId = groupId
};

builder.Services.AddLogging();

// Add Kafka EventBus
builder.Services.AddKafkaEventBus(kafkaConfiguration);

// Register event handlers
builder.Services.AddEventBusSubscriptions(Assembly.GetExecutingAssembly());

// Add the hosted service
builder.Services.AddHostedService<BankingBackgroundService>();

// Register Postgres database services using Entity Framework
var connectionString = builder.Configuration.GetConnectionString("ReadingModels")
    ?? throw new InvalidOperationException("ReadingModels Connection string is not configured");

// Add Postgres Banking Projections. dependencies
builder.Services.AddBankingDependencies(connectionString);

builder.Services.AddFeatureHandlers();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Starting Banking Consumer Service...");
logger.LogInformation("📡 Kafka Bootstrap Servers: {BootstrapServers}", kafkaBootstrapServers);
logger.LogInformation("👥 Consumer Group ID: {GroupId}", groupId);
logger.LogInformation("📋 Topic Prefix: {TopicPrefix}", topicPrefix);

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Application terminated unexpectedly");
    throw;
}
finally
{
    logger.LogInformation("🛑 Banking Consumer Service stopped");
}
