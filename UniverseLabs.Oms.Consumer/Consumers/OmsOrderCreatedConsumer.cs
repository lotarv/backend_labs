using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Models.Dto.V1.Requests;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UniverseLabs.Common;
using UniverseLabs.Messages;
using UniverseLabs.Oms.Consumer.Clients;
using UniverseLabs.Oms.Consumer.Config;

namespace UniverseLabs.Oms.Consumer.Consumers;

public class 
    OmsOrderCreatedConsumer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RabbitMqSettings> _rabbitMqSettings;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;

    public OmsOrderCreatedConsumer(IOptions<RabbitMqSettings> rabbitMqSettings, IServiceProvider serviceProvider)
    {
        _rabbitMqSettings = rabbitMqSettings;
        _serviceProvider = serviceProvider;
        _factory = new ConnectionFactory
        {
            HostName = rabbitMqSettings.Value.HostName,
            Port = rabbitMqSettings.Value.Port
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await _factory.CreateConnectionAsync(cancellationToken);
        var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        _channel = channel;
        await channel.QueueDeclareAsync(
            queue: _rabbitMqSettings.Value.OrderCreatedQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        _consumer = new AsyncEventingBasicConsumer(channel);
        _consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = message.FromJson<OrderCreatedMessage>();

            Console.WriteLine("Received: " + message);

            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<OmsClient>();
            await client.LogOrder(new V1AuditLogOrderRequest
            {
                Orders = order.OrderItems.Select(x =>
                    new V1AuditLogOrderRequest.LogOrder
                    {
                        OrderId = order.Id,
                        OrderItemId = x.Id,
                        CustomerId = order.CustomerId,
                        OrderStatus = "Created"
                    }).ToArray()
            }, CancellationToken.None);
        };

        await channel.BasicConsumeAsync(
            queue: _rabbitMqSettings.Value.OrderCreatedQueue,
            autoAck: true,
            consumer: _consumer,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _connection?.Dispose();
        _channel?.Dispose();
    }
}
