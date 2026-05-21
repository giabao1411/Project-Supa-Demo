public interface IEmailVerificationService
{
    Task SendVerifyEmailAsync(User user);

    Task<VerifyEmailTokenStatus> VerifyEmailTokenAsync(string token);
}