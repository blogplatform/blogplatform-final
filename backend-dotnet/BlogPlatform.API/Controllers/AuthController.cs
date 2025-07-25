using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(MongoDbContext context, IAuthService authService, IConfiguration configuration)
    {
        _context = context;
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] UserCreate userCreate)
    {
        // Check if email already exists
        var existingUser = await _context.Users.Find(u => u.Email == userCreate.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            return BadRequest(new { detail = "Email already registered" });
        }

        // Create new user
        var user = new User
        {
            Username = userCreate.Username,
            Email = userCreate.Email,
            PasswordHash = _authService.HashPassword(userCreate.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.InsertOneAsync(user);

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] UserLogin userLogin)
    {
        var user = await _authService.AuthenticateUserAsync(userLogin.Email, userLogin.Password);
        if (user == null)
        {
            return Unauthorized(new { detail = "Incorrect email or password" });
        }

        var accessToken = _authService.CreateAccessToken(user.Email);
        var refreshToken = _authService.CreateRefreshToken(user.Email);

        // Update user with refresh token
        var update = Builders<User>.Update.Set(u => u.RefreshToken, refreshToken);
        await _context.Users.UpdateOneAsync(u => u.Id == user.Id, update);

        // Set refresh token cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            MaxAge = TimeSpan.FromDays(int.Parse(_configuration["JWT:RefreshTokenExpireDays"] ?? "15"))
        };
        Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "bearer",
            ExpiresIn = int.Parse(_configuration["JWT:AccessTokenExpireMinutes"] ?? "30") * 60,
            Message = "Login successful",
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshResponse>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { detail = "Authentication required. Please login again." });
        }

        var email = await _authService.ValidateTokenAsync(refreshToken);
        if (email == null)
        {
            Response.Cookies.Delete("refresh_token");
            return Unauthorized(new { detail = "Invalid or expired refresh token. Please login again." });
        }

        var user = await _authService.GetUserByEmailAsync(email);
        if (user == null || user.RefreshToken != refreshToken)
        {
            Response.Cookies.Delete("refresh_token");
            return Unauthorized(new { detail = "Invalid or expired refresh token. Please login again." });
        }

        var newAccessToken = _authService.CreateAccessToken(user.Email);
        var newRefreshToken = _authService.CreateRefreshToken(user.Email);

        // Update user with new refresh token
        var update = Builders<User>.Update.Set(u => u.RefreshToken, newRefreshToken);
        await _context.Users.UpdateOneAsync(u => u.Id == user.Id, update);

        // Set new refresh token cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            MaxAge = TimeSpan.FromDays(int.Parse(_configuration["JWT:RefreshTokenExpireDays"] ?? "15"))
        };
        Response.Cookies.Append("refresh_token", newRefreshToken, cookieOptions);

        return Ok(new RefreshResponse
        {
            AccessToken = newAccessToken,
            TokenType = "bearer",
            ExpiresIn = int.Parse(_configuration["JWT:AccessTokenExpireMinutes"] ?? "30") * 60,
            Message = "Token refreshed successfully"
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> Logout()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            var user = await _authService.GetUserByEmailAsync(email);
            if (user != null)
            {
                var update = Builders<User>.Update.Set(u => u.RefreshToken, (string?)null);
                await _context.Users.UpdateOneAsync(u => u.Id == user.Id, update);
            }
        }

        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Ok(new MessageResponse { Message = "Successfully logged out" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserResponse>> GetUserById(string userId)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(userId, out _))
        {
            return BadRequest(new { detail = "Invalid user ID format" });
        }

        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            return NotFound(new { detail = "User not found" });
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("update-username")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> UpdateUsername([FromBody] UsernameUpdate usernameUpdate)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return NotFound();
        }

        // Check if username is already taken
        var existingUser = await _context.Users.Find(u => u.Username == usernameUpdate.Username && u.Id != currentUser.Id).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            return BadRequest(new { detail = "Username already taken" });
        }

        var update = Builders<User>.Update.Set(u => u.Username, usernameUpdate.Username);
        await _context.Users.UpdateOneAsync(u => u.Id == currentUser.Id, update);

        var updatedUser = await _context.Users.Find(u => u.Id == currentUser.Id).FirstOrDefaultAsync();
        return Ok(new UserResponse
        {
            Id = updatedUser!.Id,
            Username = updatedUser.Username,
            Email = updatedUser.Email,
            CreatedAt = updatedUser.CreatedAt
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> ChangePassword([FromBody] PasswordChange passwordChange)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return NotFound();
        }

        if (!_authService.VerifyPassword(passwordChange.CurrentPassword, currentUser.PasswordHash))
        {
            return BadRequest(new { detail = "Current password is incorrect" });
        }

        var newPasswordHash = _authService.HashPassword(passwordChange.NewPassword);
        var update = Builders<User>.Update.Set(u => u.PasswordHash, newPasswordHash);
        await _context.Users.UpdateOneAsync(u => u.Id == currentUser.Id, update);

        return Ok(new MessageResponse { Message = "Password changed successfully. Please login again." });
    }
}