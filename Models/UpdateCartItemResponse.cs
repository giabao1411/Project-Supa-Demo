public class UpdateCartItemResponse
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal CartTotal { get; set; }
}