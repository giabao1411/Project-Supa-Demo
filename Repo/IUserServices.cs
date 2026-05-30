using Microsoft.AspNetCore.Mvc.RazorPages;

public interface IUserService
{
    Task<PageResult<UserDTO>> GetAllAsync(int page=1,int pageSize=10);
    
    Task<UserDTO> GetByIdAsync(Guid? id);
    Task DeleteAsync(string id);
    Task UpdateAsync ( UserDTO dto);
    Task UpdatePasswordAsync(string userId, string newPassword,string oldPassword);

    Task CreateAsync(UserDTO dto);

    Task<List<RoleDTO>> GetAllUserRolesAsync();

    Task<PageResult<UserDTO>> GetUsersByKeyWordAsync(string keyword,string roleId="",int page=1,int pageSize=10);

    Task<User> FindOrCreateUserOAuth2(string? email, string? name, string provider, string providerId, string? avatar);
   
   
}