using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Models.Dto.V1.Requests;
using System.Threading;
using UniverseLabs.Messages;
using UniverseLabs.Oms.Consumer.Base;
using UniverseLabs.Oms.Consumer.Clients;
using UniverseLabs.Oms.Consumer.Config;

namespace UniverseLabs.Oms.Consumer.Consumers;

public class BatchOmsOrderCreatedConsumer(
    IOptions<RabbitMqSettings> rabbitMqSettings,
    IServiceProvider serviceProvider)
    : BaseBatchMessageConsumer<OrderCreatedMessage>(rabbitMqSettings.Value, settings => settings.OrderCreated)
{
    private int _batchCounter;

    protected override async Task ProcessMessages(OrderCreatedMessage[] messages)
    {
        if (Interlocked.Increment(ref _batchCounter) % 5 == 0)
        {
            throw new InvalidOperationException("Simulated batch failure");
        }

        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<OmsClient>();

        await client.LogOrder(new V1AuditLogOrderRequest
        {
            Orders = messages.SelectMany(order => order.OrderItems.Select(ol =>
                new V1AuditLogOrderRequest.LogOrder
                {
                    OrderId = order.Id,
                    OrderItemId = ol.Id,
                    CustomerId = order.CustomerId,
                    OrderStatus = order.OrderStatus
                })).ToArray()
        }, CancellationToken.None);
    }
}
