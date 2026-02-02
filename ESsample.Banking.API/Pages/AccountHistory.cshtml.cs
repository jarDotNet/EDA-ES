using ESsample.Banking.API.Domain.Events;
using ESsample.Banking.API.Features.GetAccountHistory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ESsample.Banking.API.Pages;

public class AccountHistoryModel : PageModel
{
    private readonly IGetAccountHistoryHandler _getAccountHistoryHandler;
    private readonly ILogger<AccountHistoryModel> _logger;

    public AccountHistoryModel(IGetAccountHistoryHandler getAccountHistoryHandler, ILogger<AccountHistoryModel> logger)
    {
        _getAccountHistoryHandler = getAccountHistoryHandler;
        _logger = logger;
    }

    [BindProperty]
    public string AccountId { get; set; } = string.Empty;

    public List<AccountEvent> AccountHistory { get; set; } = [];
    public string ErrorMessage { get; set; } = string.Empty;
    public bool HasSearched { get; set; } = false;

    public void OnGet()
    {
        // Empty initial page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        HasSearched = true;
        ErrorMessage = string.Empty;
        AccountHistory.Clear();

        if (string.IsNullOrWhiteSpace(AccountId))
        {
            ErrorMessage = "Please enter a valid Account ID.";
            return Page();
        }

        if (!Guid.TryParse(AccountId, out var accountGuid))
        {
            ErrorMessage = "The Account ID must be a valid GUID (format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";
            return Page();
        }

        try
        {
            var request = new GetAccountHistoryRequest(accountGuid);
            var response = await _getAccountHistoryHandler.HandleAsync(request);
            
            if (response.Success && response.Events is not null)
            {

                AccountHistory = response.Events.Select(e => new AccountEvent
                {
                    EventType = e.Type,
                    Timestamp = e.Timestamp,
                    AccountId = AccountId,
                    // Extract amount and balance from event data
                    Amount = ExtractAmountFromEventData(e.Data),
                    Balance = ExtractBalanceFromEventData(e.Data)
                }).ToList();
                
                if (!AccountHistory.Any())
                {
                    ErrorMessage = $"No history found for the account {AccountId}.";
                }
            }
            else
            {
                ErrorMessage = response.ErrorMessage ?? "Unknown error retrieving account history.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing request: {ex.Message}";
            _logger.LogError(ex, "Exception getting account history for {AccountId}", AccountId);
        }

        return Page();
    }

    private decimal? ExtractAmountFromEventData(object eventData)
    {
        try
        {
            decimal? amount = eventData switch
            {
                AccountCreated created => created.Transaction.Amount,
                MoneyDeposited deposited => deposited.Transaction.Amount,
                MoneyWithdrawn withdrawn => withdrawn.Transaction.Amount,
                _ => null
            };

            return amount;
        }
        catch
        {
            return null;
        }
    }

    private decimal? ExtractBalanceFromEventData(object eventData)
    {
        try
        {
            decimal? balance = eventData switch
            {
                AccountCreated created => created.InitialBalance,
                MoneyDeposited deposited => deposited.Transaction.ClosingBalance,
                MoneyWithdrawn withdrawn => withdrawn.Transaction.ClosingBalance,
                _ => null
            };

            return balance;
        }
        catch
        {
            return null;
        }
    }
}

public class AccountEvent
{
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Balance { get; set; }
    public string AccountId { get; set; } = string.Empty;
}
