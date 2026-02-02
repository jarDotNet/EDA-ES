using EventBus;

namespace Projections.Banking.Consumer.Services;

public class BankingBackgroundService : BackgroundService
{
    private readonly IEventBusConsumer _eventBusConsumer;
    private readonly IServiceProvider _serviceProvider;
    //private readonly KafkaConfiguration _kafkaConfig;
    private readonly ILogger<BankingBackgroundService> _logger;

    private string ServiceName => nameof(BankingBackgroundService);

    public BankingBackgroundService(
        IEventBusConsumer eventBusConsumer,
        IServiceProvider serviceProvider,
        //IOptions<KafkaConfiguration> kafkaConfig,
        ILogger<BankingBackgroundService> logger)
    {
        _eventBusConsumer = eventBusConsumer;
        _serviceProvider = serviceProvider;
        //_kafkaConfig = kafkaConfig.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎯 {ServiceName} starting...", ServiceName);

        try
        {
            _eventBusConsumer.Subscribe();

            _logger.LogInformation("✅ {ServiceName} started successfully", ServiceName);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken); // Adjust the delay as needed
                await _eventBusConsumer.ConsumeAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🛑 {ServiceName} stopping due to cancellation", ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ {ServiceName} encountered an error", ServiceName);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 {ServiceName} stopping...", ServiceName);
        
        try
        {
            _logger.LogInformation("✅ {ServiceName} stopped successfully", ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping {ServiceName}", ServiceName);
        }
        
        await base.StopAsync(cancellationToken);
    }
}
