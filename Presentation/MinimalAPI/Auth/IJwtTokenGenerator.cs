namespace MinimalAPI.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(string username);
}
