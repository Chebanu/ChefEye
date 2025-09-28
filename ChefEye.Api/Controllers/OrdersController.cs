using ChefEye.Contracts.Http.Request;
using ChefEye.Domain.Commands;
using ChefEye.Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChefEye.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest("Request body is null");

        var command = new CreateOrderCommand
        {
            Customer = User.Identity.Name,
            OrderMenuItemsDto = request.OrderMenuItems.Select(omi => new OrderMenuItemDto
            {
                MenuItemId = omi.MenuItemId,
                Quantity = omi.Quantity
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success)
            return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);

        return BadRequest(result.Errors);
    }

    [Authorize]
    [HttpPatch("cancel-order")]
    public async Task<IActionResult> CancelOrder([FromQuery] Guid orderId, CancellationToken ct)
    {
        var command = new CancelOrderCommand { OrderId = orderId, User = User.Identity.Name };
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
            return BadRequest(result.Error);

        return Ok("Your order has successully been canceled");
    }

    //[Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.Success || result.OrderResponses == null || result.OrderResponses.Count == 0)
            return NotFound(result.Errors);

        return Ok(result.OrderResponses.First());
    }
}