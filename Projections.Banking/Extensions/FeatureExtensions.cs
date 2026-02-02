using Microsoft.Extensions.DependencyInjection;
using Projections.Banking.Features.Balances.AccountBalanceUpdatedProjection;
using Projections.Banking.Features.Balances.AccountCreatedProjection;
using Projections.Banking.Features.Balances.GetBalances;
using Projections.Banking.Features.Transactions.CreateTransactionProjection;
using Projections.Banking.Features.Transactions.GetAccountTransactions;

namespace Projections.Banking.Extensions;

public static class FeatureExtensions
{
    public static void AddFeatureHandlers(this IServiceCollection services)
    {
        // Register all feature handlers
        services.AddScoped<IGetBalancesHandler, GetBalancesHandler>();
        services.AddScoped<IAccountCreatedProjectionHandler, AccountCreatedProjectionHandler>();
        services.AddScoped<IAccountBalanceUpdatedProjectionHandler, AccountBalanceUpdatedProjectionHandler>();
        services.AddScoped<IGetAccountTransactionsHandler, GetAccountTransactionsHandler>();
        services.AddScoped<ICreateTransactionProjectionHandler, CreateTransactionProjectionHandler>();
    }
}
