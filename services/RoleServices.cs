
using Microsoft.EntityFrameworkCore;

public class RoleServices : IRoleServices
{
    private readonly AppDbContext _db;
    public RoleServices(AppDbContext db)
    {
        _db = db;
    }
    public async Task CreateAsync(RoleDTO dto)
    {
        try
        {
            var role= new Role
            {
                Name=dto.Name,
                Description=dto.Description
            };
            _db.Add(role);

            if (dto.PermissionIds.Count() != 0)
            {
                foreach (var permission in dto.PermissionIds)
                {
                     var rolePermissions = new RolePermission
                     {
                         RoleId=role.Id,
                         PermissionId=Guid.Parse(permission),
                     };
                   await _db.AddAsync(rolePermissions);

                }
            }
            await _db.SaveChangesAsync();
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var role = await GetByIdAsync(id);

            foreach (var permissionId in role.PermissionIds)
            {
                _db.Remove(new RolePermission
                {
                    RoleId = Guid.Parse(role.Id),
                    PermissionId = Guid.Parse(permissionId),
                });

            }

            _db.Remove(new Role
            {
                Id = Guid.Parse(role.Id),
                Name = role.Name,
                Description = role.Description,
            });
        await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            
            throw new Exception(ex.Message);
        }
    }

    public async Task<List<RoleDTO>> GetAllAsync()
    {
       try
       {
        var roles = await _db.Roles.Select(u => new RoleDTO
        {
            Id=u.Id.ToString(),
            Name=u.Name,
            Description=u.Description,

        }).ToListAsync();
        return roles;
       }
       catch (Exception ex)
       {
        
        throw new Exception(ex.Message);
       };
    }

    public async Task<RoleDTO> GetByIdAsync(string id)
    {
        try
        {
            var role = await _db.Roles.Where(x => x.Id.ToString() == id )
            .Select(u => new RoleDTO
            {
                Id = u.Id.ToString(),
                Name=u.Name,
                Description = u.Description,
                PermissionIds = u.RolePermissions.Select(r => r.PermissionId.ToString()).ToList()
            }).FirstOrDefaultAsync();
            return role;
        }
        catch (Exception ex)
        {
            
            throw new Exception(ex.Message);
        }
    }

    public async Task UpdateAsync(RoleDTO dto)
    {
        try
        {
            var role = new Role
            {
                Id = Guid.Parse(dto.Id),
                Name= dto.Name,
                Description=dto.Description,
            };
            _db.Update(role);

           var oldPermission = await _db.RolePermissions.Where( x => x.RoleId== role.Id).ToListAsync();

           _db.RolePermissions.RemoveRange(oldPermission);
           
           foreach (var permissionId in dto.PermissionIds)
            {
                _db.Add(new RolePermission
                {
                    RoleId = Guid.Parse(dto.Id),
                    PermissionId = Guid.Parse(permissionId),
                });

            }

         await   _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            
            throw new Exception(ex.Message);
        }
    }
}