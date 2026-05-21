using Microsoft.EntityFrameworkCore;

public class PasswordResetService : IPasswordResetService
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    public PasswordResetService(
        AppDbContext dbContext,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _configuration = configuration;
    }
    public async Task SendPasswordResetEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return;
        }

        var oldTokens = await _dbContext.PasswordResetTokens
                            .Where(t => t.UserId == user.Id && !t.IsUsed).ToListAsync();
        foreach (var t in oldTokens)
        {
            t.IsUsed = true;
        }

        var token = TokenGenerator.GererateRefesherToken();
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            
        };
        _dbContext.PasswordResetTokens.Add(resetToken);
        await _dbContext.SaveChangesAsync();

        var resetUrl = $"{_configuration["App:FrontendUrl"]}/ResetPassword.html?token={Uri.EscapeDataString(token)}&userId={user.Id}";
        var html = $@"
            <h2>Reset Password</h2>
            <p>Hello,</p>
            <p>Please click the link below to reset your password:</p>
            <p>
                <a href='{resetUrl}'>Reset Password</a>
            </p>
            <p>This link will expire in 15 minutes.</p>
        ";
        await _emailService.SendEmailAsync(user.Email, "Reset Your Password", html);
    }

    public async Task ResetPasswordAsync(Guid userId, string token , string newPassword)
    {
        var resetToken = await _dbContext.PasswordResetTokens.Include(x=>x.User).FirstOrDefaultAsync(t =>t.Token == token && t.UserId == userId && !t.IsUsed);
        if(resetToken == null || resetToken.ExpiredAt < DateTime.UtcNow )
        {
            throw new Exception("Invalid or expired token.");
        }

        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        resetToken.IsUsed = true;

        await _dbContext.SaveChangesAsync();
    }
}