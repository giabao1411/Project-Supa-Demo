using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> CheckoutAsync(Guid userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // 1. Lấy cart
            var cart = await _db.Carts
                .Where(x => x.UserId == userId)
                .Select(c => new
                {
                    c.Id,
                    Items = c.Items.Select(i => new
                    {
                        i.ProductId,
                        i.Quantity,
                        i.PriceSnapshot
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (cart == null || !cart.Items.Any())
                throw new Exception("Cart is empty");
           
            foreach (var item in cart.Items)
            {
                var affectedRows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Products""
            SET ""Stock"" = ""Stock"" - {item.Quantity}
            WHERE ""Id"" = {item.ProductId} AND ""Stock"" >= {item.Quantity}
        ");

                if (affectedRows == 0)
                {
                    throw new Exception("Không đủ hàng");
                }
            }
            // 2. Lấy product để snapshot name + image
            var productIds = cart.Items.Select(i => i.ProductId).ToList();

            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id)).Include(x=> x.Images)
                .ToDictionaryAsync(p => p.Id);

            // 3. Tạo Order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                Items = new List<OrderItem>()
            };

            decimal total = 0;

            foreach (var item in cart.Items)
            {
                var product = products[item.ProductId];

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,

                    // 🔥 QUAN TRỌNG
                    PriceSnapshot = item.PriceSnapshot,

                    ProductNameSnapshot = product.Name,
                    ImageUrlSnapshot = product.Images.Where(i => i.IsMain).Select(i =>i.Url).FirstOrDefault()
                };

                total += item.Quantity * item.PriceSnapshot;

                order.Items.Add(orderItem);
            }

            order.TotalAmount = total;

            // 4. Save
            _db.Orders.Add(order);

            // 5. Clear cart
            var cartItems = _db.CartItems.Where(x => x.CartId == cart.Id);
            _db.CartItems.RemoveRange(cartItems);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return order.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
             Console.WriteLine(ex.InnerException?.Message);
            throw;
        }
    }

    public async Task<PageResult<OrderDTO>> GetPagedOrder(string keyWord, int page = 1, int pageSize = 10)
    {
        var orders =  _db.Orders
        .Include(x => x.User)
        .AsQueryable();

        if (!string.IsNullOrEmpty(keyWord))
        {
            orders = orders.Where(x=>x.User.Email.Contains(keyWord.Trim()));
        }
        
        var total = await  orders.CountAsync();

        var data = await orders.OrderByDescending(x =>x.CreatedAt)
        .Skip((page-1)* pageSize)
        .Take(pageSize)
        .Select( x=> new OrderDTO
        {
            Id=x.Id.ToString(),
            CustomerName = x.User.Email,
            Date = x.CreatedAt,
            Status= x.Status,
            Amount = x.TotalAmount
            

        }).ToListAsync();

    return new PageResult<OrderDTO>
    {
        Items = data,
        TotalItems=total,
        Page=page,
        PageSize=pageSize,

    };
    }
}