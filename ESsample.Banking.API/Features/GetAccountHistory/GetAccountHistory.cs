using EventSourcing.Abstractions;

namespace ESsample.Banking.API.Features.GetAccountHistory;

// Request/Response
public record GetAccountHistoryRequest(
    Guid AccountId,
    long? FromVersion = null
);

public record GetAccountHistoryResponse(
    bool Success,
    IList<EventDto>? Events = null,
    string? ErrorMessage = null
);

// DTO
public record EventDto(
    Guid Id,
    string Type,
    DateTime Timestamp,
    object Data
);

// Endpoint
public static class GetAccountHistoryEndpoint
{
    public static void MapGetAccountHistoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}/history", async (
            Guid id,
            long? fromVersion,
            IGetAccountHistoryHandler handler) =>
        {
            var request = new GetAccountHistoryRequest(id, fromVersion);
            var result = await handler.HandleAsync(request);

            return result.Success
                ? Results.Ok(result.Events)
                : Results.BadRequest(result.ErrorMessage);
        })
        .WithName("GetAccountHistory")
        .WithTags("Accounts")
        .WithOpenApi();
    }
}

// Handler
public interface IGetAccountHistoryHandler
{
    Task<GetAccountHistoryResponse> HandleAsync(GetAccountHistoryRequest request, CancellationToken cancellationToken = default);
}

public class GetAccountHistoryHandler : IGetAccountHistoryHandler
{
    private readonly IEventStore _eventStore;

    public GetAccountHistoryHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<GetAccountHistoryResponse> HandleAsync(GetAccountHistoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var events = request.FromVersion.HasValue
                ? await _eventStore.GetEventsAsync(request.AccountId, request.FromVersion.Value, cancellationToken)
                : await _eventStore.GetEventsAsync(request.AccountId, cancellationToken);

            var eventDtos = events.Select(change => new EventDto(
                change.Id,
                change.Type.Name,
                change.CreatedAt,
                change.Content
            )).ToList();

            return new GetAccountHistoryResponse(true, Events: eventDtos);
        }
        catch (Exception ex)
        {
            return new GetAccountHistoryResponse(false, ErrorMessage: $"An error occurred: {ex.Message}");
        }
    }
}
