using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

[Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var orderId = await _orderService.CheckoutAsync(userId);

        return Ok(new
        {
            orderId,
            message = "Checkout success"
        });
    }
    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult>GetPageOrders(string Keyword="",int page=1,int pageSize = 10)
    {
        var orders = await _orderService.GetPagedOrder(Keyword, page, pageSize);
        return Ok(orders);
    }
}