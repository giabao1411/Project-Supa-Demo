using System.ComponentModel.DataAnnotations;

public class User
{
    public Guid Id { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? AvatarUrl {get;set;} = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }

    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; }

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }

    // public User(int id, string passwordhash, string email)
    // {
    //     Id = id;
    //     PasswordHash = passwordhash ;
    //     Email = email;
    //     CreatedAt = DateTime.Now;
    // }
}