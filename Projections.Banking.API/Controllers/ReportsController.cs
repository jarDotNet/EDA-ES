using Microsoft.AspNetCore.Mvc;
using Projections.Banking.Features.Balances.GetBalances;
using Projections.Banking.Features.Transactions.GetAccountTransactions;

namespace Projections.Banking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    /// <summary>
    /// Gets account balances information
    /// </summary>
    /// <param name="handler">Handler to process the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account transactios information</returns>
    [HttpGet("balances")]
    [ProducesResponseType(typeof(BalanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalances(IGetBalancesHandler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.HandleAsync(cancellationToken);

        return result.Success
            ? Ok(result.Balances)
            : NotFound(result.ErrorMessage);
    }

    /// <summary>
    /// Gets account transactions information by ID
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="handler">Handler to process the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account transactios information</returns>
    [HttpGet("transactions/{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountTransactions(Guid id, IGetAccountTransactionsHandler handler, CancellationToken cancellationToken = default)
    {
        var request = new GetAccountTransactionsRequest(id);
        var result = await handler.HandleAsync(request, cancellationToken);

        return result.Success
            ? Ok(result.Transactions)
            : NotFound(result.ErrorMessage);
    }
}
