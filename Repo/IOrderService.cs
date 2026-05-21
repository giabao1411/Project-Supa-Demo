public interface IOrderService
{
    Task<Guid> CheckoutAsync(Guid userId);

    Task<PageResult<OrderDTO>> GetPagedOrder(string keyWord, int page=1,int pageSize=10);
}