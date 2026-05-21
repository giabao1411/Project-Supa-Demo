public interface IPasswordResetService
{
    Task SendPasswordResetEmailAsync(string email);

    Task ResetPasswordAsync(Guid userId, string token, string newPassword);
}