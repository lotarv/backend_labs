using Backend.BLL.Models;
using Backend.DAL.Interfaces;
using Backend.DAL.Models;
using Microsoft.Extensions.Options;
using UniverseLabs.Messages;
using BllOrderItemUnit = Backend.BLL.Models.OrderItemUnit;
using MessagesOrderItemUnit = UniverseLabs.Messages.OrderItemUnit;
using WebApi.Config;

namespace Backend.BLL.Services;

public class OrderService(
    UnitOfWork unitOfWork,
    IOrderRepository orderRepository,
    IOrderItemRepository orderItemRepository,
    RabbitMqService rabbitMqService,
    IOptions<RabbitMqSettings> rabbitMqSettings)
{
    /// <summary>
    /// Метод создания заказов
    /// </summary>
    public async Task<OrderUnit[]> BatchInsert(OrderUnit[] orderUnits, CancellationToken token)
    {
        var now = DateTimeOffset.UtcNow;
        await using var transaction = await unitOfWork.BeginTransactionAsync(token);

        try
        {
            var ordersToInsert = await orderRepository.BulkInsert(orderUnits.Select(o => new V1OrderDal
            {
                CustomerId = o.CustomerId,
                DeliveryAddress = o.DeliveryAddress,
                TotalPriceCents = o.TotalPriceCents,
                TotalPriceCurrency = o.TotalPriceCurrency,
                CreatedAt = now,
                UpdatedAt = now
            }).ToArray(), token);

            var orderMap =
                ordersToInsert.ToDictionary(x => (x.CustomerId, x.DeliveryAddress, x.TotalPriceCents, x.TotalPriceCurrency));
        
            foreach (var orderUnit in orderUnits)
            {
                orderUnit.Id = orderMap[(orderUnit.CustomerId, orderUnit.DeliveryAddress, orderUnit.TotalPriceCents, orderUnit.TotalPriceCurrency)].Id;
            }
        
            var orderItems = await orderItemRepository.BulkInsert(orderUnits.SelectMany(x => x.OrderItems.Select(a =>
                new V1OrderItemDal
                {
                    OrderId = x.Id,
                    ProductId = a.ProductId,
                    Quantity = a.Quantity,
                    ProductTitle = a.ProductTitle,
                    ProductUrl = a.ProductUrl,
                    PriceCents = a.PriceCents,
                    PriceCurrency = a.PriceCurrency,
                    CreatedAt = now,
                    UpdatedAt = now
                })).ToArray(), token);

            await transaction.CommitAsync(token);
        
            var orderItemLookup = orderItems.ToLookup(x => x.OrderId);
        
            var orders = Map(ordersToInsert, orderItemLookup);

            var messages = orders.Select(x => new OrderCreatedMessage
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                DeliveryAddress = x.DeliveryAddress,
                TotalPriceCents = x.TotalPriceCents,
                TotalPriceCurrency = x.TotalPriceCurrency,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                OrderItems = x.OrderItems.Select(i => new MessagesOrderItemUnit
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    ProductTitle = i.ProductTitle,
                    ProductUrl = i.ProductUrl,
                    PriceCents = i.PriceCents,
                    PriceCurrency = i.PriceCurrency,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                }).ToArray()
            }).ToArray();

            await rabbitMqService.Publish(
                messages,
                rabbitMqSettings.Value.OrderCreatedQueue,
                token);

            return orders;
        }
        catch (Exception e) 
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }
    
    /// <summary>
    /// Метод получения заказов
    /// </summary>
    public async Task<OrderUnit[]> GetOrders(QueryOrderItemsModel model, CancellationToken token)
    {
        var orders = await orderRepository.Query(new QueryOrdersDalModel
        {
            Ids = model.Ids,
            CustomerIds = model.CustomerIds,
            Limit = model.PageSize,
            Offset = (model.Page - 1) * model.PageSize
        }, token);

        if (orders.Length is 0)
        {
            return [];
        }
        
        ILookup<long, V1OrderItemDal> orderItemLookup = null;
        if (model.IncludeOrderItems)
        {
            var orderItems = await orderItemRepository.Query(new QueryOrderItemsDalModel
            {
                OrderIds = orders.Select(x => x.Id).ToArray(),
            }, token);

            orderItemLookup = orderItems.ToLookup(x => x.OrderId);
        }

        return Map(orders, orderItemLookup);
    }
    
    private OrderUnit[] Map(V1OrderDal[] orders, ILookup<long, V1OrderItemDal> orderItemLookup = null)
    {
        return orders.Select(x => new OrderUnit
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            DeliveryAddress = x.DeliveryAddress,
            TotalPriceCents = x.TotalPriceCents,
            TotalPriceCurrency = x.TotalPriceCurrency,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            OrderItems = orderItemLookup?[x.Id].Select(o => new BllOrderItemUnit
            {
                Id = o.Id,
                OrderId = o.OrderId,
                ProductId = o.ProductId,
                Quantity = o.Quantity,
                ProductTitle = o.ProductTitle,
                ProductUrl = o.ProductUrl,
                PriceCents = o.PriceCents,
                PriceCurrency = o.PriceCurrency,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            }).ToArray() ?? []
        }).ToArray();
    }
}
