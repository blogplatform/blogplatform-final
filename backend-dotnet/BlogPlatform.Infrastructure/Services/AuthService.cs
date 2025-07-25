using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace BlogPlatform.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly int _accessTokenExpireMinutes;
    private readonly int _refreshTokenExpireDays;

    public AuthService(IMongoDatabase database, IConfiguration configuration)
    {
        _users = database.GetCollection<User>("users");
        _configuration = configuration;
        _secretKey = _configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _accessTokenExpireMinutes = int.Parse(_configuration["JWT:AccessTokenExpireMinutes"] ?? "30");
        _refreshTokenExpireDays = int.Parse(_configuration["JWT:RefreshTokenExpireDays"] ?? "15");
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public string CreateAccessToken(string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", email),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpireMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string CreateRefreshToken(string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", email),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddDays(_refreshTokenExpireDays),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var email = jwtToken.Claims.First(x => x.Type == "sub").Value;

            return email;
        }
        catch
        {
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }
        return user;
    }
}