using System.Security;
using Microsoft.EntityFrameworkCore;

public class PermissionServices : IPermissionServices
{
    private readonly AppDbContext _db;
    

    public PermissionServices(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(PermissionDTO dto)
    {
        var permission = new Permission
        {
            
            Code=dto.Name,
            Description=dto.Description
        };
        _db.Add(permission);
       await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var permission = await _db.Permissions.FirstAsync(u =>u.Id.ToString() == id);
        if(permission==null)
            throw new Exception("Not found role to delete!");
        _db.Remove(permission);
        await _db.SaveChangesAsync();    
    }

    public async Task<List<PermissionDTO>> GetAllAsync()
    {
        var roles = await _db.Permissions.Select(u => new PermissionDTO
        {
            Id = u.Id.ToString(),
            Name = u.Code,
            Description = u.Description,
        }).ToListAsync();
        return roles;
    }

    public async Task<PermissionDTO> GetByIdAsync(string id)
    {
        var permissions = await _db.Permissions.Where(x => x.Id.ToString() == id).Select(u => new PermissionDTO
        {
            Id=u.Id.ToString(),
            Name=u.Code,
            Description=u.Description
        }).FirstAsync();
        if(permissions==null)
            throw new Exception("Not Found User");
        return permissions;
    }

    public async Task UpdateAsync(PermissionDTO dto)
    {
       var role = await _db.Permissions.FirstOrDefaultAsync(u => u.Id.ToString()== dto.Id);
       if(role == null)
        {
            throw new Exception("Not found role");
        }
        
    role.Code= dto.Name;
    role.Description = dto.Description;
     await _db.SaveChangesAsync();
    }
}