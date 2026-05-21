public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = "";
    public DateTime ExpiredAt { get; set; }

    public bool IsUsed{ get; set; }

    public User User { get; set; } = null!;
}