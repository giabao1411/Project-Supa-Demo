public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; }

    public Guid ProductId { get; set; }

    public string ProductNameSnapshot { get; set; }
    public decimal PriceSnapshot { get; set; }
    public string ImageUrlSnapshot { get; set; }

    public int Quantity { get; set; }
}