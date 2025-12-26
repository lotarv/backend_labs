using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.Dto.V1.Requests;
using UniverseLabs.Messages;
using UniverseLabs.Oms.Consumer.Base;
using UniverseLabs.Oms.Consumer.Clients;
using UniverseLabs.Oms.Consumer.Config;

namespace UniverseLabs.Oms.Consumer.Consumers;

public class BatchOmsOrderStatusChangedConsumer(
    IOptions<KafkaSettings> kafkaSettings,
    IServiceProvider serviceProvider,
    ILogger<BaseBatchKafkaConsumer<OrderStatusChangedMessage>> logger)
    : BaseBatchKafkaConsumer<OrderStatusChangedMessage>(kafkaSettings, kafkaSettings.Value.OmsOrderStatusChangedTopic, logger)
{
    protected override async Task ProcessMessages(Message<OrderStatusChangedMessage>[] messages, CancellationToken token)
    {
        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<OmsClient>();

        await client.LogOrder(new V1AuditLogOrderRequest
        {
            Orders = messages.SelectMany(message => message.Body.OrderItemIds.Select(orderItemId =>
                new V1AuditLogOrderRequest.LogOrder
                {
                    OrderId = message.Body.OrderId,
                    OrderItemId = orderItemId,
                    CustomerId = message.Body.CustomerId,
                    OrderStatus = message.Body.OrderStatus
                })).ToArray()
        }, token);
    }
}
