public interface ICartServices
{
    Task AddToCart(Guid userId, AddToCartRequest request);
    Task<CartDTO> GetCart(Guid userId);
    Task RemoveItem(Guid userId, Guid itemId);
    
    Task<UpdateCartItemResponse> UpdateQuantityAsync(Guid userId, AddToCartRequest request);
}