using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    private readonly IRoleServices _roleService;

    private readonly IPermissionServices _permissionService;

    public AdminController(IUserService userService,IPermissionServices permissionService,IRoleServices roleServices)
    {
        _userService = userService;
        _permissionService= permissionService;
        _roleService = roleServices;
    }
    [Authorize(Policy ="USER_UPDATE")]
    [HttpGet("list-users")]
    public async Task<IActionResult> GetAllUsers(int page=1,int pageSize=10)
    {
        var users = await _userService.GetAllAsync(page,pageSize);
        return Ok(users);
    }
    [Authorize(Policy ="USER_UPDATE")]
    [HttpPost("user-by-id")]
    public async Task<IActionResult> GetUserById([FromBody] Guid id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
    [Authorize(Policy="USER_UPDATE")]
    [HttpDelete("users-delete")]
    public async Task<IActionResult> DeleteUser([FromBody] string id )
    {
        try
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
    [Authorize(Policy="USER_UPDATE")]
    [HttpPut("users-update")]
    public async Task<IActionResult> UpdateUser([FromBody]UserDTO dto)
    {
        if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
        try
        {
            await _userService.UpdateAsync(dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Policy = "USER_UPDATE")]
    [HttpPost("create-users")]
    public async Task<IActionResult> CreateUser( [FromBody] UserDTO dto)
    {
        try
        {
            if (!dto.coffirmPassword.Equals(dto.password))
            {
                return BadRequest("Password and confirm password do not match");
            }
            await _userService.CreateAsync(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Policy = "USER_UPDATE")]
    [HttpGet("list-roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _userService.GetAllUserRolesAsync();
        return Ok(roles);
    }
    [Authorize(Policy = "USER_UPDATE")]
    [HttpPost("users-search")]
    public async Task<IActionResult> GetUsersByKeyWord([FromBody] SearchDTO searchDTO)
    {
        var keyWord = searchDTO.Keyword;
        var roleId = searchDTO.RoleId;
        var page = searchDTO.Page;
        var pageSize = searchDTO.PageSize;
        var users = await _userService.GetUsersByKeyWordAsync(keyWord,roleId,page,pageSize);
        return Ok(users);
    }


    [Authorize(Policy ="USER_UPDATE")]
    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermission()
    {
        var roles = await _permissionService.GetAllAsync();
        return Ok(roles);
    }

    [Authorize(Policy="USER_UPDATE")]
    [HttpPut("permissions-update")]
    public async Task<IActionResult> UpdatePermission([FromBody]PermissionDTO dto)
    {
        try
        {
            await _permissionService.UpdateAsync(dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Policy="USER_UPDATE")]
    [HttpDelete("permissions-delete")]
    public async Task<IActionResult> DeletePermission([FromBody] string id )
    {
        try
        {
            await _permissionService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
    [Authorize(Policy ="USER_UPDATE")]
    [HttpPost("permissions-by-id")]
    public async Task<IActionResult> GetPermissionById([FromBody] string id)
    {
        try
        {
            var user = await _permissionService.GetByIdAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Authorize(Policy = "USER_UPDATE")]
    [HttpPost("permissions-create")]
    public async Task<IActionResult> CreatePermission( [FromBody] PermissionDTO dto)
    {
        try
        {
            
            await _permissionService.CreateAsync(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Policy ="USER_UPDATE")]
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRole()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }
    [Authorize(Policy ="USER_UPDATE")]
    [HttpPost("roles-create")]
    public async Task<IActionResult> CreateRole ([FromBody] RoleDTO dto)
    {
        try
        {
            await _roleService.CreateAsync(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Policy ="USER_UPDATE")]
    [HttpPost("roles-by-id")]
    public async Task<IActionResult> GetRoleById([FromBody] string id)
    {
        try
        {
            var user = await _roleService.GetByIdAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
    [Authorize(Policy="USER_UPDATE")]
    [HttpPut("roles-update")]
    public async Task<IActionResult> UpdateRole([FromBody]RoleDTO dto)
    {
        try
        {
            await _roleService.UpdateAsync(dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Policy="USER_UPDATE")]
    [HttpDelete("roles-delete")]
    public async Task<IActionResult> DeleteRole([FromBody] string id )
    {
        try
        {
            await _roleService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}