public  class RefreshTokenServices
{
    private readonly AppDbContext _db;
    public RefreshTokenServices(AppDbContext db)
    {
        _db = db;
    }
    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId,HttpContext context , int daysToExpire = 7)
    {
        var refreshToken = new RefreshToken
        {
           
            UserId = userId,
            Token = TokenGenerator.GererateRefesherToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            Revoked = false,
            CreatedAt = DateTime.UtcNow,
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            IpAddress =context.Connection.RemoteIpAddress?.ToString(), 
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return refreshToken;
    }
}