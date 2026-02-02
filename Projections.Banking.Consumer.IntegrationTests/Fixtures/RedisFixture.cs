using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace Projections.Banking.Consumer.IntegrationTests.Fixtures;

public class RedisFixture : IAsyncLifetime
{
    public RedisContainer RedisContainer = default!;
    public IConnectionMultiplexer Connection = default!;

    public RedisFixture()
    {
        RedisContainer = new RedisBuilder()
            .WithImage("redis:7.0")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();

        Connection = await ConnectionMultiplexer.ConnectAsync(RedisContainer.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        if (Connection is not null)
        {
            await Connection.CloseAsync();
            await Connection.DisposeAsync();
        }
        if (RedisContainer is not null)
        {
            await RedisContainer.DisposeAsync();
        }
    }

    //public static string GetKey(string channel)
    //{
    //    var index = channel.IndexOf(':');

    //    if (index >= 0 && index < channel.Length - 1)
    //    {
    //        return channel[(index + 1)..];
    //    }

    //    return channel;
    //}
}
