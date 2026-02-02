using EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace EventSourcing.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventStore(this IServiceCollection services, string connectionString)
    {
        // Add Entity Framework DbContext
        services.AddDbContext<EventStoreDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                npgsqlOptions.ExecutionStrategy(d => new NpgsqlRetryingExecutionStrategy(d));
            }));

        // Register Event Store services
        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddScoped(typeof(IAggregateRepository<>), typeof(AggregateRepository<>));

        return services;
    }

    public static async Task<IServiceProvider> EnsureEventStoreDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await Task.Delay(0); // Simulate async operation

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
            context.Database.Migrate();
        }

        return serviceProvider;
    }
}
