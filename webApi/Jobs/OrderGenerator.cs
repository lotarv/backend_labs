using AutoFixture;
using Backend.BLL.Models;
using Backend.BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend.Jobs;

public class OrderGenerator(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fixture = new Fixture();
        var random = new Random();
        using var scope = serviceProvider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var orders = Enumerable.Range(1, 50)
                .Select(_ =>
                {
                    var orderItem = fixture.Build<OrderItemUnit>()
                        .With(x => x.PriceCurrency, "RUB")
                        .With(x => x.PriceCents, 1000)
                        .Create();

                    var order = fixture.Build<OrderUnit>()
                        .With(x => x.TotalPriceCurrency, "RUB")
                        .With(x => x.TotalPriceCents, 1000)
                        .With(x => x.OrderItems, [orderItem])
                        .Create();

                    return order;
                })
                .ToArray();

            var createdOrders = await orderService.BatchInsert(orders, stoppingToken);

            var updateCount = random.Next(1, createdOrders.Length + 1);
            var orderIdsToUpdate = createdOrders
                .OrderBy(_ => random.Next())
                .Take(updateCount)
                .Select(x => x.Id)
                .ToArray();

            var nextStatus = random.Next(0, 2) == 0 ? "processing" : "cancelled";
            await orderService.UpdateOrdersStatus(orderIdsToUpdate, nextStatus, stoppingToken);

            await Task.Delay(250, stoppingToken);
        }
    }
}
