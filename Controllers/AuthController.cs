using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/auth")]
public class AuthorController : ControllerBase
{
   

    private readonly IEmailVerificationService _emailVerificationService;

    private readonly RefreshTokenServices _refreshTokenServices;

    private readonly IPasswordResetService _passwordResetService;

    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    public AuthorController( RefreshTokenServices refreshTokenServices
    , IEmailVerificationService emailVerificationService,
    IPasswordResetService passwordResetService, IUserService userService, IAuthService authService)
    {
        
        _refreshTokenServices = refreshTokenServices;
        _emailVerificationService = emailVerificationService;
        _passwordResetService = passwordResetService;
        _userService = userService;
        _authService = authService;

    }
    
    [HttpGet("me")]

    public async Task<IActionResult> GetCurrentUser()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _userService.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
        {
            return NotFound("User not found");
        }
        return Ok(user);
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDTO userDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(userDto, HttpContext);
            var token = result.AccessToken;
            var refreshToken = result.RefreshToken;
            Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.ExpiresAt,
                SameSite = SameSiteMode.Strict,
                Secure = true,
            });
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(2),
                SameSite = SameSiteMode.Strict,
                Secure = true,
            });
            return Ok("Registration successful, please verify your email");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        // var email = userDto.Email;
        // var password = userDto.password;
        // var confirmPassword = userDto.coffirmPassword;
        // if(password != confirmPassword)
        // {
        //     return BadRequest("Password and Confirm Password do not match");
        // }
        // if (await _db.Users.AnyAsync(u => u.Email == email))
        // {
        //     return BadRequest("Email already exists");
        // }
        // var newUser = new User
        // {
        //     Email = email,
        //     PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        //     CreatedAt = DateTime.UtcNow,
        //     IsEmailVerified = false
        // };

        // _db.Users.Add(newUser);
        // await _db.SaveChangesAsync();
        // // Default role assignment
        // var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        // if (role == null)
        //     throw new Exception("Default role not found");
        // var userRole = new UserRole
        // {
        //     UserId = newUser.Id,
        //     RoleId = role.Id
        // };
        // _db.UserRoles.Add(userRole);
        // await _db.SaveChangesAsync();
        // // await _emailVerificationService.SendVerifyEmailAsync(newUser);
        // //redirect to home page with jwt token and refresh token
        // //create JWT token here (omitted for brevity)
        // var token = await GererateJwtToken(newUser);
        // // refresh token
        // var refreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(newUser.Id, HttpContext);
        //set refresh token in http only cookie


    }
    // [HttpPost("register-swagger")]
    // public async Task<IActionResult> Register(string email, string password)
    // {

    //     if (await _db.Users.AnyAsync(u => u.Email == email))
    //     {
    //         return BadRequest("Email already exists");
    //     }
    //     var newUser = new User
    //     {
    //         Email = email,
    //         PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
    //         CreatedAt = DateTime.UtcNow,
    //         IsEmailVerified = false
    //     };

    //     _db.Users.Add(newUser);
    //     await _db.SaveChangesAsync();
    //     // Default role assignment
    //     var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
    //     if (role == null)
    //         throw new Exception("Default role not found");
    //     var userRole = new UserRole
    //     {
    //         UserId = newUser.Id,
    //         RoleId = role.Id
    //     };
    //     _db.UserRoles.Add(userRole);
    //     await _db.SaveChangesAsync();
    //     await _emailVerificationService.SendVerifyEmailAsync(newUser);
    //     return Ok("Registration successful, please verify your email");
    // }
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token is required");
        }
        var result = await _emailVerificationService.VerifyEmailTokenAsync(token);
        return result switch
        {
            VerifyEmailTokenStatus.Valid => Ok("Email verified successfully"),
            VerifyEmailTokenStatus.Expired => BadRequest("Token has expired"),
            VerifyEmailTokenStatus.Used => BadRequest("Token has already been used"),
            VerifyEmailTokenStatus.NotFound => BadRequest("Invalid token"),
            _ => BadRequest("An error occurred during email verification")
        };
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDTO userDto)
    {
        // var email = userDto.Email;
        // var password = userDto.password;

        // var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        // if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        // {
        //     return Unauthorized("Invalid credentials");
        // }
        // //create JWT token here (omitted for brevity)
        // var token = await GererateJwtToken(user);
        // // refresh token
        // var refreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(user.Id, HttpContext);
        //set refresh token in http only cookie
        try
        {
            var result = await _authService.LoginAsync(userDto, HttpContext);
            var token = result.AccessToken;
            var refreshToken = result.RefreshToken;
            Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.ExpiresAt,
                SameSite = SameSiteMode.Strict,
                Secure = true,
            });
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(5),
                SameSite = SameSiteMode.Strict,
                Secure = true,
            });
            return Ok(new { token });
        }
        catch (Exception ex)
        {

            return BadRequest(ex.Message);
        }
    }

    // [HttpPost("login-swagger")]
    // public async Task<IActionResult> Login(string email, string password)
    // {

    //     var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    //     if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
    //     {
    //         return Unauthorized("Invalid credentials");
    //     }
    //     //create JWT token here (omitted for brevity)
    //     var token = await GererateJwtToken(user);
    //     // refresh token
    //     var refreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(user.Id, HttpContext);
    //     //set refresh token in http only cookie
    //     Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
    //     {
    //         HttpOnly = true,
    //         Expires = refreshToken.ExpiresAt,
    //         SameSite = SameSiteMode.Strict,
    //         Secure = true,
    //     });
    //     Response.Cookies.Append("access_token", token, new CookieOptions
    //     {
    //         HttpOnly = true,
    //         Expires = DateTime.UtcNow.AddMinutes(5),
    //         SameSite = SameSiteMode.Strict,
    //         Secure = true,
    //     });
    //     return Ok(new { token });
    // }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            var oldRefreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(oldRefreshToken))
                return Unauthorized("No refresh token provided");

            // var storedToken = await _db.RefreshTokens
            // .Include(x => x.User)
            // .FirstOrDefaultAsync(x => x.Token == oldRefreshToken && !x.Revoked && x.ExpiresAt > DateTime.UtcNow);

            // if (storedToken == null)
            // {
            //     return Unauthorized("Invalid refresh token");
            // }
            // storedToken.Revoked = true;
            // storedToken.RevokedAt = DateTime.UtcNow;
            // await _db.SaveChangesAsync();

            // var newRefreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(storedToken.UserId, HttpContext);
            // var newAccessToken = await GererateJwtToken(storedToken.User);
            var result = await _authService.RefreshAsync(oldRefreshToken, HttpContext);
            var newAccessToken = result.AccessToken;
            var newRefreshToken = result.RefreshToken;
            Response.Cookies.Append("refresh_token", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.ExpiresAt,
                SameSite = SameSiteMode.Strict,
                Secure = true,
            });
            Response.Cookies.Append("access_token", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(5),
                SameSite = SameSiteMode.Strict,
                Secure = true,
            });

            return Ok(new { token = newAccessToken });
        }
        catch (Exception ex)
        {

            return BadRequest(ex.Message);
        }


    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        await _passwordResetService.SendPasswordResetEmailAsync(email);
        return Ok("If the email is registered, a password reset link has been sent.");
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        try
        {
            if (!string.Equals(resetPasswordDTO.NewPassword, resetPasswordDTO.ConfirmPassword))
            {
                return BadRequest("Password and Confirm Password do not match");
            }
            await _passwordResetService.ResetPasswordAsync(resetPasswordDTO.UserId, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);
            return Ok("Password has been reset successfully");
        }
        catch (System.Exception ex)
        {

            return BadRequest(ex.Message);
        }
    }
    //Create API Logout 
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User not found");
            }
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("No refresh token provided");
            }
            await _authService.LogoutAsync(refreshToken);
            HttpContext.Response.Cookies.Delete("refresh_token");
            Response.Cookies.Delete("access_token");

            return Ok("Logged out successfully");
        }
        catch (Exception ex)
        {

            return BadRequest(ex.Message);
        }


        // var storedToken = await _db.RefreshTokens
        // .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.Revoked && x.ExpiresAt > DateTime.UtcNow);
        // if (storedToken == null)
        // {
        //     return BadRequest("Invalid refresh token");
        // }

        // storedToken.Revoked = true;
        // storedToken.RevokedAt = DateTime.UtcNow;

        // await _db.SaveChangesAsync();

    }

    //Function test policy
    [Authorize(Policy = "USER_DELETE")]
    [HttpDelete("delete-user")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        try
        {
            await _userService.DeleteAsync(userId);
            return Ok("User deleted successfully");

        }
        catch
        {
            return BadRequest("Error deleting user");
        }
    }

    [Authorize(Policy = "USER_UPDATE")]
    [HttpPut("update-user")]
    public async Task<IActionResult> UpdateUser(string userId, string email, string password)
    {
        try
        {
            var userDTO = new UserDTO
            {
                Id = Guid.Parse(userId),
                Email = email,
                password = password
            };
            await _userService.UpdateAsync(userDTO);
            return Ok("User updated successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);

        }
        // var oldUser = await _db.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        // if (oldUser == null)
        // {
        //     return NotFound("User not found");
        // }
        // var emailExist = await _db.Users.AnyAsync(u => u.Email == email && u.Id.ToString() != userId);
        // if (emailExist)
        // {
        //     return BadRequest("Email already in use");
        // }
        // oldUser.Email = email;
        // oldUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        // await _db.SaveChangesAsync();


        // return Ok("User updated successfully");
    }
    [Authorize]
    [HttpPut("update-user-password")]
    public async Task<IActionResult> UpdateUserPassword(UserDTO userDTO)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized("User not found");
            }
            if(userDTO.password != userDTO.coffirmPassword)
            {
                return BadRequest("New password and confirm password do not match");
            }
            await _userService.UpdatePasswordAsync(userId, userDTO.password, userDTO.currentPassword);

            return Ok("Password updated successfully");
        }
        catch (Exception ex)
        {

            return BadRequest(ex.Message);
        }


    }

    [Authorize(Policy = "USER_VIEW")]
    [HttpGet("views-user")]
    public async Task<IActionResult> ViewsUser()
    {

        var listUser = await _userService.GetAllAsync();

        return Ok(listUser);
    }
    [Authorize(Policy = "USER_CREATE")]
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser()
    {


        return Ok("Users create successfully");
    }


   


}