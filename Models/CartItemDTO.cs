public class CartItemDTO
{
   public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    public string ProductName { get; set; }
    public string ImageUrl { get; set; }

    public int Quantity { get; set; }
    public decimal PriceSnapshot { get; set; }

    public decimal Total => Quantity * PriceSnapshot;
}