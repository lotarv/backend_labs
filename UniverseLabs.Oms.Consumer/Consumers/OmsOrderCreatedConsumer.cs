using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.Dto.V1.Requests;
using UniverseLabs.Messages;
using UniverseLabs.Oms.Consumer.Base;
using UniverseLabs.Oms.Consumer.Clients;
using UniverseLabs.Oms.Consumer.Config;

namespace UniverseLabs.Oms.Consumer.Consumers;

public class OmsOrderCreatedConsumer(
    IOptions<KafkaSettings> kafkaSettings,
    IServiceProvider serviceProvider,
    ILogger<BaseKafkaConsumer<OrderCreatedMessage>> logger)
    : BaseKafkaConsumer<OrderCreatedMessage>(kafkaSettings, kafkaSettings.Value.OmsOrderCreatedTopic, logger)
{
    protected override async Task ProcessMessage(Message<OrderCreatedMessage> message, CancellationToken token)
    {
        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<OmsClient>();

        await client.LogOrder(new V1AuditLogOrderRequest
        {
            Orders = message.Body.OrderItems.Select(ol =>
                new V1AuditLogOrderRequest.LogOrder
                {
                    OrderId = message.Body.Id,
                    OrderItemId = ol.Id,
                    CustomerId = message.Body.CustomerId,
                    OrderStatus = message.Body.OrderStatus
                }).ToArray()
        }, token);
    }
}
