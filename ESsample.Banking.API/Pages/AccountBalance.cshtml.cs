using ESsample.Banking.API.Features.GetAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ESsample.Banking.API.Pages;

public class AccountBalanceModel : PageModel
{
    private readonly IGetAccountHandler _getAccountHandler;
    private readonly ILogger<AccountBalanceModel> _logger;

    public AccountBalanceModel(IGetAccountHandler getAccountHandler, ILogger<AccountBalanceModel> logger)
    {
        _getAccountHandler = getAccountHandler;
        _logger = logger;
    }

    [BindProperty]
    public string AccountId { get; set; } = string.Empty;

    public AccountBalance? AccountBalance { get; set; }
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
        AccountBalance = null;

        if (string.IsNullOrWhiteSpace(AccountId))
        {
            ErrorMessage = "Please, enter a valid Account ID.";
            return Page();
        }

        if (!Guid.TryParse(AccountId, out var accountGuid))
        {
            ErrorMessage = "The Account ID must be a valid GUID (format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";
            return Page();
        }

        try
        {
            var request = new GetAccountRequest(accountGuid);
            var response = await _getAccountHandler.HandleAsync(request);
            
            if (!response.Success)
            {
                ErrorMessage = response.ErrorMessage ?? "Unknown error while retrieving balance.";
            }

            if (response.Account is null)
            {
                ErrorMessage = $"No balance found for the account {AccountId}.";
            }

            if (response.Success && response.Account is not null)
            {

                AccountBalance = new AccountBalance
                {
                    AccountNumber = response.Account.AccountNumber,
                    OwnerName = response.Account.OwnerName,
                    Balance = response.Account.Balance,
                    Version = response.Account.Version,
                    IsActive = response.Account.IsActive    
                };
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing request: {ex.Message}";
            _logger.LogError(ex, "Exception getting account history for {AccountId}", AccountId);
        }

        return Page();
    }
}

public class AccountBalance
{
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public long Version { get; set; }
    public bool IsActive { get; set; }
}
