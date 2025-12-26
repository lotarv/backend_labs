using Backend.BLL.Models;
using Backend.BLL.Services;
using Backend.Validators;
using Microsoft.AspNetCore.Mvc;
using Models.Dto.V1.Requests;
using Models.Dto.V1.Responses;

namespace WebApi.Controllers.V1;

[Route("api/v1/audit")]
public class AuditLogOrderController(
    AuditLogOrderService auditLogOrderService,
    ValidatorFactory validatorFactory) : ControllerBase
{
    [HttpPost("log-order")]
    public async Task<ActionResult<V1AuditLogOrderResponse>> V1LogOrder(
        [FromBody] V1AuditLogOrderRequest request,
        CancellationToken token)
    {
        var validationResult = await validatorFactory
            .GetValidator<V1AuditLogOrderRequest>()
            .ValidateAsync(request, token);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var inserted = await auditLogOrderService.BatchInsert(
            request.Orders.Select(x => new AuditLogOrderUnit
            {
                OrderId = x.OrderId,
                OrderItemId = x.OrderItemId,
                CustomerId = x.CustomerId,
                OrderStatus = x.OrderStatus
            }).ToArray(),
            token);

        return Ok(new V1AuditLogOrderResponse
        {
            Inserted = inserted
        });
    }
}
