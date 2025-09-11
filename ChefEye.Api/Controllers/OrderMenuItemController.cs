using Microsoft.AspNetCore.Mvc;
using MediatR;
using ChefEye.Domain.Commands;

namespace ChefEye.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderMenuItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderMenuItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateMenuItemCommandResult>> CreateMenuItem([FromBody] CreateMenuItemCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Sucsess)
            return BadRequest(result);

        return CreatedAtAction(nameof(CreateMenuItem), new { id = result.MenuItemId }, result);
    }
}