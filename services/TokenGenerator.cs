using System.Security.Cryptography;

public static class TokenGenerator
{
    public static string GererateRefesherToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
    
}