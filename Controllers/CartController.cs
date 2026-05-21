using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartServices _cartService;

    public CartController(ICartServices cartService)
    {
        _cartService = cartService;
    }

    [Authorize]
    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            await _cartService.AddToCart(GetUserId(), request);
            return Ok("Added to cart");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartService.GetCart(GetUserId());
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("remove/{itemId}")]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        try
        {
            await _cartService.RemoveItem(GetUserId(), itemId);
            return Ok("Removed");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("update-quantity")]
    public async Task<IActionResult> UpdateQuantity([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();

        var item = await _cartService.UpdateQuantityAsync(userId, request);

        return Ok(item);
    }
}