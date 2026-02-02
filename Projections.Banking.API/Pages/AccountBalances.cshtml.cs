using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projections.Banking.Features.Balances.GetBalances;

namespace Projections.Banking.API.Pages;

public class AccountBalancesModel : PageModel
{
    private readonly IGetBalancesHandler _getBalancesHandler;
    private readonly ILogger<AccountBalancesModel> _logger;

    public AccountBalancesModel(IGetBalancesHandler getBalancesHandler, ILogger<AccountBalancesModel> logger)
    {
        _getBalancesHandler = getBalancesHandler;
        _logger = logger;
    }

    public List<BalanceDto> Balances { get; set; } = [];
    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        ErrorMessage = string.Empty;
        Balances.Clear();

        try
        {
            var response = await _getBalancesHandler.HandleAsync();

            if (response.Success && response.Balances is not null)
            {

                if (response.Balances.Any())
                {
                    Balances = [.. response.Balances];
                }
                else
                {
                     ErrorMessage = $"No account balances found.";
               }
            }
            else
            {
                ErrorMessage = response.ErrorMessage ?? "Unknown error retrieving account balances.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing request: {ex.Message}";
            _logger.LogError(ex, "Exception getting account balances");
        }

        return Page();
    }
}
