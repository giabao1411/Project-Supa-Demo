public class ExternalLogin
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = default!;   
    public string ProviderKey { get; set; } = default!; 
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}