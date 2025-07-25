using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/likes")]
public class LikesController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IAuthService _authService;

    public LikesController(MongoDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("blogs/{blogId}")]
    [Authorize]
    public async Task<ActionResult> ToggleLike(string blogId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(blogId))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

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

        var blog = await _context.Blogs.Find(b => b.Id == blogId).FirstOrDefaultAsync();
        if (blog == null)
        {
            return NotFound(new { detail = "Blog not found" });
        }

        var existingLike = await _context.Likes.Find(l => l.BlogId == blogId && l.UserId == currentUser.Id).FirstOrDefaultAsync();

        if (existingLike != null)
        {
            // Remove like
            await _context.Likes.DeleteOneAsync(l => l.Id == existingLike.Id);
            var blogUpdate = Builders<Blog>.Update.Inc(b => b.LikesCount, -1);
            await _context.Blogs.UpdateOneAsync(b => b.Id == blogId, blogUpdate);

            return Ok(new { message = "Like removed successfully" });
        }
        else
        {
            // Add like
            var like = new Like
            {
                BlogId = blogId,
                UserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Likes.InsertOneAsync(like);
            var blogUpdate = Builders<Blog>.Update.Inc(b => b.LikesCount, 1);
            await _context.Blogs.UpdateOneAsync(b => b.Id == blogId, blogUpdate);

            return Ok(new LikeResponse
            {
                Id = like.Id,
                BlogId = like.BlogId,
                UserId = like.UserId,
                CreatedAt = like.CreatedAt
            });
        }
    }

    [HttpGet("blogs/{blogId}/count")]
    public async Task<ActionResult<int>> GetBlogLikesCount(string blogId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(blogId))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

        var count = await _context.Likes.CountDocumentsAsync(l => l.BlogId == blogId);
        return Ok((int)count);
    }

    [HttpGet("blogs/{blogId}/my-like")]
    [Authorize]
    public async Task<ActionResult<LikeResponse>> GetMyLikeForBlog(string blogId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(blogId))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

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

        var like = await _context.Likes.Find(l => l.BlogId == blogId && l.UserId == currentUser.Id).FirstOrDefaultAsync();
        if (like == null)
        {
            return NotFound(new { detail = "You haven't liked this blog" });
        }

        return Ok(new LikeResponse
        {
            Id = like.Id,
            BlogId = like.BlogId,
            UserId = like.UserId,
            CreatedAt = like.CreatedAt
        });
    }

    [HttpDelete("blogs/{blogId}")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> RemoveLike(string blogId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(blogId))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

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

        var result = await _context.Likes.DeleteOneAsync(l => l.BlogId == blogId && l.UserId == currentUser.Id);
        if (result.DeletedCount == 0)
        {
            return NotFound(new { detail = "No like found for this blog" });
        }

        var blogUpdate = Builders<Blog>.Update.Inc(b => b.LikesCount, -1);
        await _context.Blogs.UpdateOneAsync(b => b.Id == blogId, blogUpdate);

        return Ok(new MessageResponse { Message = "Like removed successfully" });
    }

    [HttpGet("my-likes")]
    [Authorize]
    public async Task<ActionResult<List<LikeResponse>>> GetMyLikes()
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

        var likes = await _context.Likes.Find(l => l.UserId == currentUser.Id).ToListAsync();

        var likeResponses = likes.Select(like => new LikeResponse
        {
            Id = like.Id,
            BlogId = like.BlogId,
            UserId = like.UserId,
            CreatedAt = like.CreatedAt
        }).ToList();

        return Ok(likeResponses);
    }
}