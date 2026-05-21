public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }

    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }
}
