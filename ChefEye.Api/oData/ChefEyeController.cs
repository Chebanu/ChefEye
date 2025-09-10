using ChefEye.Contracts.Http.Response;
using ChefEye.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ChefEye.Api.oData;

[Route("odata/[controller]")]
public class ChefEyeController : ODataController
{
    private readonly ChefEyeDbContext _dbContext;

    public ChefEyeController(ChefEyeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [EnableQuery]
    [HttpGet("menu-item")]
    public IQueryable<MenuItemResponse> GetMenuItem()
    {
        return _dbContext.MenuItems.Select(mi => new MenuItemResponse
        {
            Name = mi.Name,
            Description = mi.Description,
            Price = mi.Price
        });
    }

    [EnableQuery]
    [HttpGet("order")]
    public IQueryable<OrderResponse> GetOrder()
    {
        return _dbContext.Orders
            .Select(order => new OrderResponse
            {
                Customer = order.Customer.FullName,
                Receipt = order.TotalAmount,
                OrderPositions = order.OrderMenuItems.Select(omi => new OrderPosition
                {
                    MenuItemId = omi.MenuItemId,
                    Name = omi.MenuItem.Name,
                    Quantity = omi.Quantity,
                    Price = omi.MenuItem.Price
                }).ToList(),
            });
    }
}