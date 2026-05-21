public interface IAuthService
{
    Task<AuthResult> RegisterAsync(UserDTO dto, HttpContext context);
    Task<AuthResult> LoginAsync(UserDTO dto, HttpContext context);
    Task<AuthResult> RefreshAsync(string oldRefreshToken, HttpContext context);
    Task LogoutAsync(string refreshToken);
   
}