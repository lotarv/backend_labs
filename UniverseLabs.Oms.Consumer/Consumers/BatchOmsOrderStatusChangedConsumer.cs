using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Models.Dto.V1.Requests;
using UniverseLabs.Messages;
using UniverseLabs.Oms.Consumer.Base;
using UniverseLabs.Oms.Consumer.Clients;
using UniverseLabs.Oms.Consumer.Config;

namespace UniverseLabs.Oms.Consumer.Consumers;

public class BatchOmsOrderStatusChangedConsumer(
    IOptions<RabbitMqSettings> rabbitMqSettings,
    IServiceProvider serviceProvider)
    : BaseBatchMessageConsumer<OrderStatusChangedMessage>(rabbitMqSettings.Value, settings => settings.OrderStatusChanged)
{
    protected override async Task ProcessMessages(OrderStatusChangedMessage[] messages)
    {
        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<OmsClient>();

        await client.LogOrder(new V1AuditLogOrderRequest
        {
            Orders = messages.SelectMany(order => order.OrderItemIds.Select(orderItemId =>
                new V1AuditLogOrderRequest.LogOrder
                {
                    OrderId = order.OrderId,
                    OrderItemId = orderItemId,
                    CustomerId = order.CustomerId,
                    OrderStatus = order.OrderStatus
                })).ToArray()
        }, CancellationToken.None);
    }
}
