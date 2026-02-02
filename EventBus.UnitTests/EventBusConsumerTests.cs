using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace EventBus.UnitTests;

public record TestIntegrationEvent : IntegrationEvent
{
    public required string Message { get; init; }
}

public class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
{
    public Task HandleAsync(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class EventBusConsumerTestImpl(IServiceProvider serviceProvider) : EventBusConsumer(serviceProvider)
{
    public override void Subscribe() { }
    public override void Unsubscribe() { }
    public override Task ConsumeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class EventBusConsumerTests
{
    [Fact]
    public async Task EmitIntegrationEvent_ThrowsArgumentNullException_WhenEventIsNull()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var consumer = new EventBusConsumerTestImpl(serviceProvider);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            consumer.EmitIntegrationEvent<TestIntegrationEvent>(null!, serviceProvider)
        );
    }

    [Fact]
    public async Task EmitIntegrationEvent_ThrowsArgumentNullException_WhenServiceProviderIsNull()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var consumer = new EventBusConsumerTestImpl(serviceProvider);
        var testEvent = new TestIntegrationEvent { Message = "Hello, World!" };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            consumer.EmitIntegrationEvent(testEvent, null!)
        );
    }

    [Fact]
    public async Task EmitIntegrationEvent_ShouldInvokeMultipleHandlers()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent { Message = "Hello, World!" };
        var tcs1 = new TaskCompletionSource<bool>(); 
        var tcs2 = new TaskCompletionSource<bool>(); 

        var mockHandler1 = new Mock<IIntegrationEventHandler<TestIntegrationEvent>>();
        var mockHandler2 = new Mock<IIntegrationEventHandler<TestIntegrationEvent>>();
        mockHandler1
            .Setup(h => h.HandleAsync(It.IsAny<TestIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                tcs1.SetResult(true);
            })
            .Returns(Task.CompletedTask);
        mockHandler2
            .Setup(h => h.HandleAsync(It.IsAny<TestIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                tcs2.SetResult(true);
            })
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler1.Object);
        services.AddSingleton(mockHandler2.Object);

        var serviceProvider = services.BuildServiceProvider();

        var consumer = new EventBusConsumerTestImpl(serviceProvider);

        // Act
        await consumer.EmitIntegrationEvent(testEvent, serviceProvider);

        // Wait for the handler to be executed
        var timeout = TimeSpan.FromSeconds(5);
        var completedTask1 = await Task.WhenAny(tcs1.Task, Task.Delay(timeout));
        var completedTask2 = await Task.WhenAny(tcs2.Task, Task.Delay(timeout));

        // Assert
        Assert.True(completedTask1 == tcs1.Task, "Handler was not called within the timeout.");
        Assert.True(completedTask2 == tcs2.Task, "Handler was not called within the timeout.");
        mockHandler1.Verify(h => h.HandleAsync(It.Is<TestIntegrationEvent>(e => e.Message == testEvent.Message), It.IsAny<CancellationToken>()), Times.Once);
        mockHandler2.Verify(h => h.HandleAsync(It.Is<TestIntegrationEvent>(e => e.Message == testEvent.Message), It.IsAny<CancellationToken>()), Times.Once);
    }
}
