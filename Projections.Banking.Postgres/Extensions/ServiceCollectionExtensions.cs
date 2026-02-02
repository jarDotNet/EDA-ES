using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Projections.Banking.Domain.Balances;
using Projections.Banking.Domain.Transactions;
using Projections.Banking.Postgres.Repositories;

namespace Projections.Banking.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankingDependencies(this IServiceCollection services, string connectionString)
    {
        // Add Entity Framework DbContext
        services.AddDbContext<BankingDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }),
            ServiceLifetime.Transient);

        // Register Repositories
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }

    public static async Task<IServiceProvider> EnsureBankingDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await Task.Delay(0); // Simulate async operation

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
            context.Database.Migrate();
        }

        return serviceProvider;
    }
}
