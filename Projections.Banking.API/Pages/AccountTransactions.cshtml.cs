using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projections.Banking.Features.Transactions.GetAccountTransactions;

namespace Projections.Banking.API.Pages;

public class AccountTransactionsModel : PageModel
{
    private readonly IGetAccountTransactionsHandler _getAccountTransactionsHandler;
    private readonly ILogger<AccountTransactionsModel> _logger;

    public AccountTransactionsModel(IGetAccountTransactionsHandler getAccountTransactionsHandler, ILogger<AccountTransactionsModel> logger)
    {
        _getAccountTransactionsHandler = getAccountTransactionsHandler;
        _logger = logger;
    }

    [BindProperty]
    public string AccountId { get; set; } = string.Empty;

    public List<TransactionDto> Transactions { get; set; } = [];
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
        Transactions.Clear();

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
            var request = new GetAccountTransactionsRequest(accountGuid);
            var response = await _getAccountTransactionsHandler.HandleAsync(request);

            if (response.Success && response.Transactions is not null)
            {

                if (response.Transactions.Any())
                {
                    Transactions = [.. response.Transactions];
                }
                else
                {
                    ErrorMessage = $"No transactions found for the account {AccountId}.";
                }
            }
            else
            {
                ErrorMessage = response.ErrorMessage ?? "Unknown error retrieving account transactions.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing request: {ex.Message}";
            _logger.LogError(ex, "Exception getting account transactions for {AccountId}", AccountId);
        }

        return Page();
    }
}
