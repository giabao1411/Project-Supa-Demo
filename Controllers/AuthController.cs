using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;

[ApiController]
[Route("api/auth")]
public class AuthorController : ControllerBase
{
   

    private readonly IEmailVerificationService _emailVerificationService;

    private readonly RefreshTokenServices _refreshTokenServices;

    private readonly IPasswordResetService _passwordResetService;

    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    private readonly TokenService _tokenService;
    public AuthorController( RefreshTokenServices refreshTokenServices
    , IEmailVerificationService emailVerificationService,
    IPasswordResetService passwordResetService, IUserService userService, IAuthService authService, TokenService tokenService)
    {
        
        _refreshTokenServices = refreshTokenServices;
        _emailVerificationService = emailVerificationService;
        _passwordResetService = passwordResetService;
        _userService = userService;
        _authService = authService;
        _tokenService = tokenService;

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

     [HttpGet("{provider}/login")]
    public IActionResult Login(string provider)
    {
        var redirectUrl =  Url.Action("Callback", "Author", new { provider });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        
        
        var scheme = provider.ToLower() switch
        {
            "google"   => GoogleDefaults.AuthenticationScheme,
            "facebook" => FacebookDefaults.AuthenticationScheme,
            _ => throw new ArgumentException("Invalid provider")
        };
        
        return Challenge(properties, scheme);
    }

   [HttpGet("{provider}/callback")]
    public async Task<IActionResult> Callback(string provider)
    {
        string scheme;
        switch (provider.ToLower())
    {
        case "google":
            scheme = GoogleDefaults.AuthenticationScheme;
            break;
        case "facebook":
            scheme = FacebookDefaults.AuthenticationScheme;
            break;
        default:
            return BadRequest("Invalid provider");
    }

        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return BadRequest("OAuth authentication failed");

        // Lấy thông tin từ claims
        var claims     = result.Principal!.Claims;
        var email      = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "facebook_"+RandomNumberGenerator.GetInt32(1,110)+"@gmail.com";
        var name       = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var avatar     = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        // Tìm hoặc tạo user
        var user = await _userService.FindOrCreateUserOAuth2(email, name, provider, providerId!, avatar);

       
        var token = await _tokenService.GererateJwtToken(user);

        var refreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(user.Id,HttpContext);

       
        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddDays(7)
        });
        Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddDays(7)
        });

        // Redirect về trang chủ frontend
        return Redirect("https://localhost:7265/home.html");
    }
    [HttpGet("error")]
    [AllowAnonymous]
    public IActionResult Error([FromQuery] string message)
    {
        return BadRequest(new { error = message });
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
        


    }
    
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

   

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            var oldRefreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(oldRefreshToken))
                return Unauthorized("No refresh token provided");

           
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