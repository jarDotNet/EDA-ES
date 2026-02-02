using EventBus;
using StackExchange.Redis;

namespace Projections.Banking.Consumer.IntegrationTests.Handlers
{
    internal class FirstTestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        private readonly IDatabase _redisDatabase;

        public static string CacheKey = nameof(FirstTestIntegrationEventHandler);

        public FirstTestIntegrationEventHandler(IDatabase redisDatabase)
        {
            _redisDatabase = redisDatabase;
        }

        public async Task HandleAsync(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            await _redisDatabase.StringSetAsync(CacheKey, integrationEvent?.Name ?? string.Empty);
        }
    }
}
    