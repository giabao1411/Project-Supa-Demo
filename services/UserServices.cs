using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class UserServices : IUserService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    private readonly RefreshTokenServices _refreshTokenServices;

    public UserServices(AppDbContext db, TokenService tokenService, RefreshTokenServices refreshTokenServices)
    {
       _db = db;
        _tokenService = tokenService;
        _refreshTokenServices = refreshTokenServices;
    }

   public async Task<UserDTO> GetByIdAsync(Guid? id)
    {
        var user = await _db.Users.Where(u =>u.Id == id)
        .Select(u => new UserDTO
        {
            Id= u.Id,
            Email= u.Email,
            CreatedAt = u.CreatedAt,
            IsEmailVerified = u.IsEmailVerified,
            AvatarUrl = u.AvatarUrl,
            Roles = u.UserRoles.Select(ur => new RoleDTO
            {
                Id = ur.RoleId.ToString(),
                Name = ur.Role.Name,
            }).ToList(),
            Permissions = u.UserRoles.SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code).Distinct().ToList(),
        }).FirstAsync();
        if (user == null)
            throw new Exception("User not found");
        return user;
    }

    public async Task<PageResult<UserDTO>> GetAllAsync(int page=1 , int pageSize=10)
    {
        var users = await _db.Users.Select(u => new UserDTO
        {
            Id= u.Id,
            Email= u.Email,
            CreatedAt= u.CreatedAt,
            IsEmailVerified = u.IsEmailVerified,
            AvatarUrl = u.AvatarUrl,
            Roles = u.UserRoles.Select(ur => new RoleDTO
            {
                Id = ur.RoleId.ToString(),
                Name = ur.Role.Name,
            }).ToList(),

        }).Skip((page - 1) * pageSize).Take(pageSize).
        ToListAsync();
        var totalCount = await _db.Users.CountAsync();

        return new PageResult<UserDTO>
        {
            Items = users,
            TotalItems = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
    public async Task DeleteAsync(string id)
    {
        var user = await _db.Users.FindAsync(Guid.Parse(id));
        if (user == null)
            throw new Exception("User not found");
        var userRoles = await _db.UserRoles.Where(ur => ur.UserId.ToString() == id).ToListAsync();
        _db.UserRoles.RemoveRange(userRoles);    
        _db.Users.Remove(user);

        await _db.SaveChangesAsync();
    }
    public async Task UpdateAsync(UserDTO dto)
    {
       var oldUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        if (oldUser == null)
        {
            throw new Exception("User not found");
        }
        var emailExist = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != dto.Id);
        if (emailExist)
        {
           throw new Exception("Email already in use");
        }
        oldUser.Email = dto.Email;
        oldUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.password);
        if( !string.IsNullOrEmpty(dto.AvatarUrl))
        {
            if(oldUser.AvatarUrl !=null)
                FileHelper.DeleteFile(oldUser.AvatarUrl);
            oldUser.AvatarUrl = dto.AvatarUrl;
        }
        var userRoles = await _db.UserRoles.Where(ur => ur.UserId == dto.Id).ToListAsync();
        _db.UserRoles.RemoveRange(userRoles);
        if (dto.Roles.Count() != 0)
        {
            foreach (var role in dto.Roles)
            {
                var newUserRole = new UserRole
                {
                    UserId = dto.Id,
                    RoleId = Guid.Parse(role.Id)
                };
                _db.UserRoles.Add(newUserRole);
            }

        }

        await _db.SaveChangesAsync();
    }
    public async Task UpdatePasswordAsync(string userId, string newPassword, string oldPassword)
    {
        if (!IsPasswordValid(newPassword))
        {
            throw new Exception("New password does not meet complexity requirements");
        }

        var user = await _db.Users.FirstAsync(u => u.Id.ToString() == userId);
        var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);

        if (!isOldPasswordValid)
        {
            throw new Exception("Old password is incorrect");
        }
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();
    }
    private bool IsPasswordValid(string password)
    {
        var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$");
        return regex.IsMatch(password);

    }

    public async Task CreateAsync(UserDTO dto)
    {
        try
        {
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false,
                AvatarUrl = dto.AvatarUrl
            };
            _db.Users.Add(user);
           
            if (dto.Roles.Count() != 0)
            {
                foreach (var role in dto.Roles)
                {
                    var userRoles = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = Guid.Parse(role.Id)
                    };
                    _db.UserRoles.Add(userRoles);
                }

            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }

    }

    public async Task<List<RoleDTO>> GetAllUserRolesAsync()
    {
        var roles = await _db.Roles.ToListAsync();
        return roles.Select(r => new RoleDTO
        {
            Id = r.Id.ToString(),
            Name = r.Name,
            
        }).ToList();
    }
    public async Task<PageResult<UserDTO>> GetUsersByKeyWordAsync(string keyWord,string roleId="",int page=1,int pageSize=10){
        var query=  _db.Users.Where(u => u.Email.Contains(keyWord));
        if(!string.IsNullOrEmpty(roleId)){
            query = query.Where(u => _db.UserRoles.Any(ur => ur.RoleId.ToString() == roleId && ur.UserId == u.Id));
        }
         var uses = await query
        .Select(u => new UserDTO
        {
            Id= u.Id,
            Email= u.Email,
            CreatedAt= u.CreatedAt,
            IsEmailVerified = u.IsEmailVerified,
            Role = u.UserRoles.Select(ur => new RoleDTO
            {
                Id = ur.RoleId.ToString(),
                Name = ur.Role.Name,
            }).First(),

        }).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var totalCount = await query.CountAsync();
           
        return  new PageResult<UserDTO>
        {
            Items = uses,
            TotalItems = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}