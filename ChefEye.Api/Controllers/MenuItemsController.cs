using ChefEye.Contracts.Http.Request;
using ChefEye.Contracts.Http.Response;
using ChefEye.Domain.Commands;
using ChefEye.Domain.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChefEye.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateMenuItemRequest> _createMenuItemValidator;
    private readonly IValidator<UpdateMenuItemRequest> _updateMenuItemValidator;

    public MenuItemsController(IMediator mediator, IValidator<CreateMenuItemRequest> createMenuItemValidator, IValidator<UpdateMenuItemRequest> updateMenuItemValidator)
    {
        _mediator = mediator;
        _createMenuItemValidator = createMenuItemValidator;
        _updateMenuItemValidator = updateMenuItemValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _createMenuItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
            });
        }

        var command = new CreateMenuItemCommand
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.Errors != null && result.Errors.Length > 0)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetMenuItem), new { id = result.MenuItemId }, result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMenuItem([FromBody] UpdateMenuItemRequest requests, CancellationToken cancellationToken)
    {
        var validationResult = await _updateMenuItemValidator.ValidateAsync(requests, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
            });
        }

        var command = new UpdateMenuItemCommand
        {
            Id = requests.Id,
            Name = requests.Name,
            Description = requests.Description,
            Price = requests.Price
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.Errors != null && result.Errors.Length > 0)
            return BadRequest(result.Errors);

        return Ok($"Menu item {requests.Name} has been updated successfully");
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