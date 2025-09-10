using ChefEye.Domain.Commands;
using ChefEye.Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChefEye.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
            return BadRequest("Request body is null");

        var result = await _mediator.Send(command, cancellationToken);

        if (result.Errors != null && result.Errors.Length > 0)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetMenuItem), new { id = result.MenuItemId }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMenuItem(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetMenuItemQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}