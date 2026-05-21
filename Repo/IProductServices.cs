public interface IProductServices
{
   Task<PageResult<ProductDTO>> GetPaged(int page,int pageSize);
   Task<PageResult<ProductDTO>> GetAll(ProductQueryDTO query);
   Task Create(ProductCreateDTO dto);

   Task Update(Guid id , ProductCreateDTO dto);

   Task Delete(Guid id);

   Task<ProductCreateDTO> Get(Guid id);


}