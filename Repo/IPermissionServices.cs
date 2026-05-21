public interface IPermissionServices
{
    Task<List<PermissionDTO>> GetAllAsync();
    Task<PermissionDTO> GetByIdAsync(string id);
    Task CreateAsync(PermissionDTO dto);
    Task UpdateAsync(PermissionDTO dto);
    Task DeleteAsync(string id);
}