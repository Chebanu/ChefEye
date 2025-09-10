/*using ChefEye.Domain;
using ChefEye.Contracts.Http.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ChefEye.Api.oData;

//[Authorize]
[Route("odata/[controller]")]
public class OrdersOData : ODataController
{
    private readonly ChefEyeDbContext _dbContext;

    public OrdersOData(ChefEyeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [EnableQuery]
    [HttpGet("all")]
    public IQueryable<OrderResponse> Get()
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
}*/