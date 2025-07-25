using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/interests")]
[Authorize]
public class InterestsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IAuthService _authService;

    public InterestsController(MongoDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost]
    public async Task<ActionResult<UserInterestsResponse>> CreateUserInterests([FromBody] UserInterestsCreate interests)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        var existing = await _context.UserInterests.Find(ui => ui.UserId == currentUser.Id).FirstOrDefaultAsync();

        if (existing != null)
        {
            var update = Builders<UserInterests>.Update
                .Set(ui => ui.Interests, interests.Interests)
                .Set(ui => ui.UpdatedAt, now);
            
            await _context.UserInterests.UpdateOneAsync(ui => ui.UserId == currentUser.Id, update);
            
            var updated = await _context.UserInterests.Find(ui => ui.UserId == currentUser.Id).FirstOrDefaultAsync();
            return Ok(new UserInterestsResponse
            {
                Id = updated!.Id,
                UserId = updated.UserId,
                Interests = updated.Interests,
                CreatedAt = updated.CreatedAt,
                UpdatedAt = updated.UpdatedAt
            });
        }
        else
        {
            var userInterests = new UserInterests
            {
                UserId = currentUser.Id,
                Interests = interests.Interests,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.UserInterests.InsertOneAsync(userInterests);

            return Ok(new UserInterestsResponse
            {
                Id = userInterests.Id,
                UserId = userInterests.UserId,
                Interests = userInterests.Interests,
                CreatedAt = userInterests.CreatedAt,
                UpdatedAt = userInterests.UpdatedAt
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<UserInterestsResponse>> GetUserInterests()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var interests = await _context.UserInterests.Find(ui => ui.UserId == currentUser.Id).FirstOrDefaultAsync();
        if (interests == null)
        {
            return NotFound(new { detail = "User interests not found" });
        }

        return Ok(new UserInterestsResponse
        {
            Id = interests.Id,
            UserId = interests.UserId,
            Interests = interests.Interests,
            CreatedAt = interests.CreatedAt,
            UpdatedAt = interests.UpdatedAt
        });
    }

    [HttpPut]
    public async Task<ActionResult<UserInterestsResponse>> UpdateUserInterests([FromBody] UserInterestsUpdate interestsUpdate)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var existing = await _context.UserInterests.Find(ui => ui.UserId == currentUser.Id).FirstOrDefaultAsync();
        if (existing == null)
        {
            return NotFound(new { detail = "User interests not found. Create interests first." });
        }

        var update = Builders<UserInterests>.Update
            .Set(ui => ui.Interests, interestsUpdate.Interests)
            .Set(ui => ui.UpdatedAt, DateTime.UtcNow);

        await _context.UserInterests.UpdateOneAsync(ui => ui.UserId == currentUser.Id, update);

        var updated = await _context.UserInterests.Find(ui => ui.UserId == currentUser.Id).FirstOrDefaultAsync();
        return Ok(new UserInterestsResponse
        {
            Id = updated!.Id,
            UserId = updated.UserId,
            Interests = updated.Interests,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        });
    }

    [HttpPatch("add")]
    public async Task<ActionResult<MessageResponse>> AddInterest([FromQuery] string interest)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var existing = await _context.UserInterests.Find(ui => ui.UserId == currentUser.Id).FirstOrDefaultAsync();
        if (existing == null)
        {
            return NotFound(new { detail = "User interests not found. Create interests first." });
        }

        var update = Builders<UserInterests>.Update
            .AddToSet(ui => ui.Interests, interest)
            .Set(ui => ui.UpdatedAt, DateTime.UtcNow);

        await _context.UserInterests.UpdateOneAsync(ui => ui.UserId == currentUser.Id, update);

        return Ok(new MessageResponse { Message = $"Interest '{interest}' added successfully" });
    }

    [HttpPatch("remove")]
    public async Task<ActionResult<MessageResponse>> RemoveInterest([FromQuery] string interest)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var existing = await _context.UserInterests.Find(ui => ui.UserId == currentUser.Id).FirstOrDefaultAsync();
        if (existing == null)
        {
            return NotFound(new { detail = "User interests not found." });
        }

        var update = Builders<UserInterests>.Update
            .Pull(ui => ui.Interests, interest)
            .Set(ui => ui.UpdatedAt, DateTime.UtcNow);

        await _context.UserInterests.UpdateOneAsync(ui => ui.UserId == currentUser.Id, update);

        return Ok(new MessageResponse { Message = $"Interest '{interest}' removed successfully" });
    }

    [HttpDelete]
    public async Task<ActionResult<MessageResponse>> DeleteUserInterests()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var result = await _context.UserInterests.DeleteOneAsync(ui => ui.UserId == currentUser.Id);
        if (result.DeletedCount == 0)
        {
            return NotFound(new { detail = "User interests not found" });
        }

        return Ok(new MessageResponse { Message = "User interests deleted successfully" });
    }

    [HttpGet("suggestions")]
    [AllowAnonymous]
    public ActionResult<List<string>> GetInterestSuggestions()
    {
        var suggestions = new List<string>
        {
            "Technology", "Programming", "Web Development", "Mobile Development", "Data Science",
            "Machine Learning", "Artificial Intelligence", "Cybersecurity", "Cloud Computing",
            "DevOps", "Blockchain", "Cryptocurrency", "Gaming", "Design", "UI/UX",
            "Business", "Entrepreneurship", "Marketing", "Finance", "Health", "Fitness",
            "Travel", "Food", "Photography", "Music", "Movies", "Books", "Sports",
            "Science", "Education", "Politics", "Environment", "Art", "Culture",
            "Fashion", "Lifestyle", "Personal Development", "Productivity", "Innovation"
        };

        return Ok(suggestions);
    }
}