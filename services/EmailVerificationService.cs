using Microsoft.EntityFrameworkCore;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EmailVerificationService(
        AppDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }
    public async Task SendVerifyEmailAsync(User user)
    {
        var oldToken = await _context.EmailVerificationTokens
                        .Where(x=> x.UserId== user.Id && !x.IsUsed).ToListAsync();
        foreach (var t in oldToken)
        {
            t.IsUsed = true;
        }

        var token = TokenGenerator.GererateRefesherToken();
        var verifyToken = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.EmailVerificationTokens.Add(verifyToken);
        await _context.SaveChangesAsync();
        var verifyUrl= $"{_configuration["App:FrontendUrl"]}/api/auth/verify-email?token={Uri.EscapeDataString(token)}";
        var html = $@"
            <h2>Xác thực email</h2>
            <p>Xin chào,</p>
            <p>Vui lòng click vào link bên dưới để xác thực email:</p>
            <p>
                <a href='{verifyUrl}'>Xác thực email</a>
            </p>
            <p>Link sẽ hết hạn sau 15 phút.</p>
        ";
        await _emailService.SendEmailAsync(user.Email,"Xác thực email", html);
    }
    public async Task<VerifyEmailTokenStatus> VerifyEmailTokenAsync(string token)
    {
        var verifyToken = await _context.EmailVerificationTokens.Include(x=> x.User)
                            .FirstOrDefaultAsync(x => x.Token == token);

        if(verifyToken == null)
        {
            return VerifyEmailTokenStatus.NotFound;
        }
        if(verifyToken.IsUsed)
        {
            return VerifyEmailTokenStatus.Used;
        }
        if(verifyToken.ExpiredAt < DateTime.UtcNow)
        {
            return VerifyEmailTokenStatus.Expired;
        }
        verifyToken.IsUsed = true;
        verifyToken.User.IsEmailVerified = true; 
        await _context.SaveChangesAsync();
        return VerifyEmailTokenStatus.Valid;
    }
}