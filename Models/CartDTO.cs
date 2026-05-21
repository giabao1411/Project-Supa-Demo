public class CartDTO
{
     public List<CartItemDTO> Items {get ; set;} = new();

    public decimal Total {get; set;}
}