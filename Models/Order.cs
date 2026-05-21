public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public User User {get; set;}

    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }

    public string Status { get; set; } // Pending, Paid, Cancelled

    public List<OrderItem> Items { get; set; } = new();
}