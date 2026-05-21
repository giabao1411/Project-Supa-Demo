using Microsoft.EntityFrameworkCore;

public class CartService : ICartServices
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddToCart(Guid userId, AddToCartRequest request)
    {
        var product = await _db.Products.FindAsync(request.ProductId);
        if (product == null)
            throw new Exception("Product not found");

        if (product.Stock < request.Quantity)
            throw new Exception("Not enough stock");

        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };
            _db.Carts.Add(cart);
        }

        var item = _db.CartItems.FirstOrDefault(i => i.CartId == cart.Id && i.ProductId == request.ProductId);

        if (item != null)
        {
            item.Quantity += request.Quantity;
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId=cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity,
                PriceSnapshot = product.Price
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<CartDTO> GetCart(Guid userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return new CartDTO();

        var response = new CartDTO
        {
            Items = cart.Items.Select(i => new CartItemDTO
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ImageUrl = i.Product.Images
                    .Where(i => i.IsMain)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
                Quantity = i.Quantity,
                PriceSnapshot = i.PriceSnapshot
            }).ToList()
        };

        response.Total = response.Items.Sum(i => i.Total);

        return response;
    }

    public async Task RemoveItem(Guid userId, Guid itemId)
    {
        var item = await _db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == userId);

        if (item == null)
            throw new Exception("Item not found");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }
    public async Task<UpdateCartItemResponse> UpdateQuantityAsync(Guid userId, AddToCartRequest request)
    {
        var cart = await _db.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
            throw new Exception("Cart not found");

        var cartItem = await _db.CartItems
            .FirstOrDefaultAsync(x => x.CartId == cart.Id && x.ProductId == request.ProductId);

        if (cartItem == null)
            throw new Exception("Item not found");

        cartItem.Quantity = request.Quantity;

        await _db.SaveChangesAsync();
        // tính lại item total
        var itemTotal = cartItem.Quantity * cartItem.PriceSnapshot;

        // tính lại cart total
        var cartTotal = await _db.CartItems
            .Where(x => x.CartId == cart.Id)
            .SumAsync(x => x.Quantity * x.PriceSnapshot);

        return new UpdateCartItemResponse
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            ItemTotal = itemTotal,
            CartTotal = cartTotal
        };
    }
}