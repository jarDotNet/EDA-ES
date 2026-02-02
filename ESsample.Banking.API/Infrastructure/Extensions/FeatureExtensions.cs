using ESsample.Banking.API.Features.CreateAccount;
using ESsample.Banking.API.Features.DepositMoney;
using ESsample.Banking.API.Features.GetAccount;
using ESsample.Banking.API.Features.GetAccountHistory;
using ESsample.Banking.API.Features.TransferMoney;
using ESsample.Banking.API.Features.WithdrawMoney;

namespace ESsample.Banking.API.Infrastructure.Extensions;

public static class FeatureExtensions
{
    public static void AddFeatureHandlers(this IServiceCollection services)
    {
        // Register all feature handlers
        services.AddScoped<ICreateAccountHandler, CreateAccountHandler>();
        services.AddScoped<IDepositMoneyHandler, DepositMoneyHandler>();
        services.AddScoped<IWithdrawMoneyHandler, WithdrawMoneyHandler>();
        services.AddScoped<ITransferMoneyHandler, TransferMoneyHandler>();
        services.AddScoped<IGetAccountHandler, GetAccountHandler>();
        services.AddScoped<IGetAccountHistoryHandler, GetAccountHistoryHandler>();
    }

    public static void MapAccountFeatures(this IEndpointRouteBuilder app)
    {
        // Add all feature endpoints
        app.MapCreateAccountEndpoint();
        app.MapDepositMoneyEndpoint();
        app.MapWithdrawMoneyEndpoint();
        app.MapTransferMoneyEndpoint();
        app.MapGetAccountEndpoint();
        app.MapGetAccountHistoryEndpoint();
    }
}
