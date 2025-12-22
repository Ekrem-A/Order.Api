using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces;
using System.Text.Json;

namespace Order.Infrastructure.Messaging;

public class ServiceBusPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient? _client;
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, ServiceBusSender> _senders = new();

    public ServiceBusPublisher(
        IConfiguration configuration,
        ILogger<ServiceBusPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var connectionString = configuration["ServiceBus:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _client = new ServiceBusClient(connectionString);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        if (_client == null)
        {
            _logger.LogWarning("Service Bus client is not configured. Event {EventType} will not be published.", typeof(TEvent).Name);
            return;
        }

        var topicName = GetTopicName<TEvent>();
        var sender = await GetOrCreateSender(topicName);

        var messageBody = JsonSerializer.Serialize(@event);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = typeof(TEvent).Name,
            MessageId = Guid.NewGuid().ToString()
        };

        try
        {
            await sender.SendMessageAsync(message, cancellationToken);
            _logger.LogInformation("Published event {EventType} to topic {TopicName}", typeof(TEvent).Name, topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to topic {TopicName}", typeof(TEvent).Name, topicName);
            throw;
        }
    }

    private string GetTopicName<TEvent>()
    {
        var eventTypeName = typeof(TEvent).Name;

        return eventTypeName switch
        {
            "OrderCreatedIntegrationEvent" => _configuration["ServiceBus:OrderCreatedTopicName"] ?? "order-created",
            "OrderCancelledIntegrationEvent" => _configuration["ServiceBus:OrderCancelledTopicName"] ?? "order-cancelled",
            _ => eventTypeName.ToLowerInvariant()
        };
    }

    private async Task<ServiceBusSender> GetOrCreateSender(string topicName)
    {
        if (!_senders.TryGetValue(topicName, out var sender))
        {
            sender = _client!.CreateSender(topicName);
            _senders[topicName] = sender;
        }

        return await Task.FromResult(sender);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}

