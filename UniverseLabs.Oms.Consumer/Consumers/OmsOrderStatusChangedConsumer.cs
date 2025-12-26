using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.Dto.V1.Requests;
using UniverseLabs.Messages;
using UniverseLabs.Oms.Consumer.Base;
using UniverseLabs.Oms.Consumer.Clients;
using UniverseLabs.Oms.Consumer.Config;

namespace UniverseLabs.Oms.Consumer.Consumers;

public class OmsOrderStatusChangedConsumer(
    IOptions<KafkaSettings> kafkaSettings,
    IServiceProvider serviceProvider,
    ILogger<BaseKafkaConsumer<OrderStatusChangedMessage>> logger)
    : BaseKafkaConsumer<OrderStatusChangedMessage>(kafkaSettings, kafkaSettings.Value.OmsOrderStatusChangedTopic, logger)
{
    protected override async Task ProcessMessage(Message<OrderStatusChangedMessage> message, CancellationToken token)
    {
        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<OmsClient>();

        await client.LogOrder(new V1AuditLogOrderRequest
        {
            Orders = message.Body.OrderItemIds.Select(orderItemId =>
                new V1AuditLogOrderRequest.LogOrder
                {
                    OrderId = message.Body.OrderId,
                    OrderItemId = orderItemId,
                    CustomerId = message.Body.CustomerId,
                    OrderStatus = message.Body.OrderStatus
                }).ToArray()
        }, token);
    }
}
