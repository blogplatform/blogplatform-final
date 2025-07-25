using BlogPlatform.Core.Models;

namespace BlogPlatform.Infrastructure.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string CreateAccessToken(string email);
    string CreateRefreshToken(string email);
    Task<string?> ValidateTokenAsync(string token);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> AuthenticateUserAsync(string email, string password);
}