using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;
    private readonly RefreshTokenServices _refreshTokenServices;
    private readonly AppDbContext _db;

    public AuthService(IUserService userService, TokenService tokenService, RefreshTokenServices refreshTokenServices, AppDbContext db)
    {
        _userService = userService;
        _tokenService = tokenService;
        _refreshTokenServices = refreshTokenServices;
        _db = db;
    }

    public async Task<AuthResult> RegisterAsync(UserDTO userDTO, HttpContext context)
    {
        // Implementation for user registration

        var email = userDTO.Email;
        var password = userDTO.password;
        var confirmPassword = userDTO.coffirmPassword;
        if(password != confirmPassword)
        {
            throw new Exception("Password and Confirm Password do not match");
        }
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            throw new Exception("Email already exists");
        }
        var newUser = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = false
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
        // Default role assignment
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (role == null)
            throw new Exception("Default role not found");
        var userRole = new UserRole
        {
            UserId = newUser.Id,
            RoleId = role.Id
        };
        _db.UserRoles.Add(userRole);
        await _db.SaveChangesAsync();
        // await _emailVerificationService.SendVerifyEmailAsync(newUser);
        //redirect to home page with jwt token and refresh token
        //create JWT token here (omitted for brevity)
        var accessToken = await  _tokenService.GererateJwtToken(newUser);
        // refresh token
        var refreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(newUser.Id, context);
        //set refresh token in http only cookie
        return new AuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResult> LoginAsync(UserDTO userDTO, HttpContext context)
    {
        var email = userDTO.Email;
        var password = userDTO.password;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password");
        }
        var accessToken = await  _tokenService.GererateJwtToken(user);
        // refresh token
        var refreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(user.Id, context);
        return new AuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResult> RefreshAsync(string oldRefreshToken, HttpContext context)
    {
         var storedToken = await _db.RefreshTokens
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.Token == oldRefreshToken && !x.Revoked && x.ExpiresAt > DateTime.UtcNow);

        if (storedToken == null)
        {
            throw new Exception("Invalid refresh token");
        }
        storedToken.Revoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var newRefreshToken = await _refreshTokenServices.CreateRefreshTokenAsync(storedToken.UserId, context);
        var newAccessToken = await _tokenService.GererateJwtToken(storedToken.User);
        return new AuthResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
          var storedToken = await _db.RefreshTokens
        .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.Revoked && x.ExpiresAt > DateTime.UtcNow);
        if (storedToken == null)
        {
            throw new Exception("Invalid refresh token");
        }

        storedToken.Revoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

    }

   
}