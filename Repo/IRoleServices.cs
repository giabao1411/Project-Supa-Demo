public interface IRoleServices
{
    Task<List<RoleDTO>> GetAllAsync();
    Task<RoleDTO> GetByIdAsync(string id);
    Task CreateAsync( RoleDTO dto);
    Task UpdateAsync(RoleDTO dto);
    Task DeleteAsync(string id);
}