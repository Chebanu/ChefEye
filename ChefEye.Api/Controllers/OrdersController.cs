using ChefEye.Domain.Commands;
using ChefEye.Domain.Queries;
using MediatR;
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

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
            return BadRequest("Request body is null");

        var result = await _mediator.Send(command, cancellationToken);

        if (result.CreateOrderCommandResultType == CreateOrderCommandResultType.Success)
            return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);

        return BadRequest(result.Errors);
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